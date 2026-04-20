import { DestroyRef, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, Subject, Subscription, debounceTime, distinctUntilChanged, finalize, map, switchMap } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ItemApiModel, ItemsApiService } from '../../../core/api/services/items-api.service';
import { MenuApiModel, MenuItemApiModel, MenusApiService } from '../../../core/api/services/menus-api.service';
import { AdminMenuDrawer } from '../admin-menu-drawer/admin-menu-drawer';
import {
  AdminMenuDrawerData,
  AdminMenuDraftValue,
  AdminMenuEditorMode,
  AdminMenuSavePayload,
} from '../admin-menu-drawer/admin-menu.models';
import { UiNotificationsService } from '../../../core/ui/ui-notifications.service';

type AdminMenuRow = {
  id: string;
  title: string;
  description: string;
  itemsCount: number;
};

@Component({
  selector: 'app-admin-menus-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, AdminMenuDrawer],
  templateUrl: './admin-menus-section.html',
  styleUrl: './admin-menus-section.scss',
})
export class AdminMenusSection {
  private readonly destroyRef = inject(DestroyRef);
  private readonly menusApiService = inject(MenusApiService);
  private readonly itemsApiService = inject(ItemsApiService);
  private readonly uiNotificationsService = inject(UiNotificationsService);

  private readonly searchInput$ = new Subject<string>();
  private loadSubscription: Subscription | null = null;
  private drawerLoadSubscription: Subscription | null = null;
  private saveSubscription: Subscription | null = null;
  private deleteSubscription: Subscription | null = null;

  readonly pageSize = signal<number>(10);
  readonly pageIndex = signal<number>(0);
  readonly searchTerm = signal<string>('');
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string>('');
  readonly successMessage = signal<string>('');
  readonly rows = signal<AdminMenuRow[]>([]);

  readonly drawerOpen = signal<boolean>(false);
  readonly drawerMode = signal<AdminMenuEditorMode>('create');
  readonly drawerData = signal<AdminMenuDrawerData | null>(null);
  readonly drawerErrorMessage = signal<string>('');
  readonly isDrawerLoading = signal<boolean>(false);
  readonly isSavingDrawer = signal<boolean>(false);
  readonly editingMenuId = signal<string | null>(null);
  readonly deletingMenuId = signal<string | null>(null);

  private readonly paginationState = computed(() => {
    const all = this.rows();
    const size = this.pageSize();
    const index = this.pageIndex();
    return { all, size, index };
  });

  readonly totalPages = computed(() => {
    const { all, size } = this.paginationState();
    return Math.max(1, Math.ceil(all.length / size));
  });
  readonly pageLabel = computed(() => `${this.pageIndex() + 1} из ${this.totalPages()}`);
  readonly pagedRows = computed(() => {
    const { all, size, index } = this.paginationState();
    const start = index * size;
    return all.slice(start, start + size);
  });
  readonly canPrev = computed(() => this.pageIndex() > 0);
  readonly canNext = computed(() => this.pageIndex() < this.totalPages() - 1);

  constructor() {
    this.searchInput$
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.pageIndex.set(0);
        this.loadMenus(value);
      });

    this.loadMenus('');
  }

  prevPage(): void {
    if (this.canPrev()) {
      this.pageIndex.update((value) => value - 1);
    }
  }

  nextPage(): void {
    if (this.canNext()) {
      this.pageIndex.update((value) => value + 1);
    }
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.searchInput$.next(value.trim());
  }

  openCreateDrawer(): void {
    this.drawerMode.set('create');
    this.editingMenuId.set(null);
    this.drawerData.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(true);
    this.drawerOpen.set(true);

    this.drawerLoadSubscription?.unsubscribe();
    this.drawerLoadSubscription = this.itemsApiService.listItems()
      .pipe(finalize(() => this.isDrawerLoading.set(false)))
      .subscribe({
        next: (availableItems) => {
          this.drawerData.set({
            availableItems,
            draft: {
              title: '',
              description: '',
              items: [],
            },
          });
        },
        error: () => {
          this.drawerErrorMessage.set('Не удалось загрузить справочник объектов.');
        },
      });
  }

  openEditDrawer(row: AdminMenuRow): void {
    this.drawerMode.set('edit');
    this.editingMenuId.set(row.id);
    this.drawerData.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(true);
    this.drawerOpen.set(true);

    this.drawerLoadSubscription?.unsubscribe();
    this.drawerLoadSubscription = forkJoin({
      menu: this.menusApiService.getMenu(row.id),
      menuItems: this.menusApiService.listMenuItems(row.id),
      availableItems: this.itemsApiService.listItems(),
    }).pipe(
      switchMap(({ menu, menuItems, availableItems }) => {
        if (menuItems.length === 0) {
          return this.mapMenuDrawerData(menu, menuItems, availableItems);
        }

        return forkJoin(
          menuItems.map((menuItem) =>
            this.menusApiService.getMenuItemAmount(row.id, menuItem.itemId).pipe(
              map((amount) => ({ ...menuItem, amount }))
            )
          )
        ).pipe(
          switchMap((itemsWithAmount) => this.mapMenuDrawerData(menu, itemsWithAmount, availableItems))
        );
      }),
      finalize(() => this.isDrawerLoading.set(false)),
    ).subscribe({
      next: (data) => {
        this.drawerData.set(data);
      },
      error: (error: unknown) => {
        this.drawerErrorMessage.set(this.mapDrawerLoadErrorMessage(error));
      },
    });
  }

  closeDrawer(): void {
    this.drawerLoadSubscription?.unsubscribe();
    this.drawerLoadSubscription = null;
    this.drawerOpen.set(false);
    this.drawerData.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.isSavingDrawer.set(false);
    this.editingMenuId.set(null);
  }

  saveMenu(payload: AdminMenuSavePayload): void {
    this.drawerErrorMessage.set('');
    this.isSavingDrawer.set(true);
    this.saveSubscription?.unsubscribe();

    let request$: Observable<unknown>;
    if (this.drawerMode() === 'edit' && this.editingMenuId()) {
      request$ = this.updateExistingMenu(this.editingMenuId()!, payload);
    } else {
      request$ = this.createNewMenu(payload);
    }

    this.saveSubscription = request$
      .pipe(finalize(() => this.isSavingDrawer.set(false)))
      .subscribe({
        next: () => {
          const mode = this.drawerMode();
          this.closeDrawer();
          this.uiNotificationsService.success(mode === 'edit' ? 'Меню обновлено.' : 'Меню создано.');
          this.loadMenus(this.searchTerm().trim());
        },
        error: (error: unknown) => {
          this.drawerErrorMessage.set(this.mapDrawerSaveErrorMessage(error));
        },
      });
  }

  deleteMenu(row: AdminMenuRow): void {
    if (this.deletingMenuId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm(`Удалить меню "${row.title}"?`)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.deletingMenuId.set(row.id);
    this.deleteSubscription?.unsubscribe();
    this.deleteSubscription = this.menusApiService.deleteMenu(row.id)
      .pipe(finalize(() => this.deletingMenuId.set(null)))
      .subscribe({
        next: () => {
          this.rows.update((rows) => rows.filter((menu) => menu.id !== row.id));
          this.clampPageIndex();
          this.uiNotificationsService.success('Меню удалено.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteErrorMessage(error));
        },
      });
  }

  private loadMenus(titleContains: string): void {
    this.loadSubscription?.unsubscribe();
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.loadSubscription = this.menusApiService.listMenus(titleContains || undefined)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (menus) => {
          this.rows.set(menus.map((menu) => ({
            id: menu.id,
            title: menu.title,
            description: menu.description ?? '',
            itemsCount: menu.menuItems.length,
          })));
          this.clampPageIndex();
        },
        error: (error: unknown) => {
          this.rows.set([]);
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  private createNewMenu(payload: AdminMenuSavePayload): Observable<void> {
    return this.menusApiService.createMenu(payload.title, payload.description ?? undefined).pipe(
      switchMap((menu) => this.syncMenuItems(menu.id, [], payload.items))
    );
  }

  private updateExistingMenu(menuId: string, payload: AdminMenuSavePayload): Observable<void> {
    return this.menusApiService.listMenuItems(menuId).pipe(
      switchMap((currentItems) =>
        this.menusApiService.updateMenu(menuId, {
          title: payload.title,
          description: payload.description ?? undefined,
        }).pipe(
          switchMap(() => this.syncMenuItems(menuId, currentItems, payload.items))
        )
      )
    );
  }

  private syncMenuItems(
    menuId: string,
    currentItems: MenuItemApiModel[],
    updatedItems: Array<{ itemId: string; title: string; amount: number }>,
  ): Observable<void> {
    const currentMap = new Map(currentItems.map((item) => [item.itemId, item]));
    const updatedMap = new Map(updatedItems.map((item) => [item.itemId, item]));
    const operations: Observable<unknown>[] = [];

    for (const updated of updatedItems) {
      const current = currentMap.get(updated.itemId);
      if (!current) {
        operations.push(this.menusApiService.addMenuItem(menuId, updated.itemId, updated.amount));
      } else if (current.amount !== updated.amount) {
        operations.push(this.menusApiService.updateMenuItemAmount(menuId, updated.itemId, updated.amount));
      }
    }

    for (const current of currentItems) {
      if (!updatedMap.has(current.itemId)) {
        operations.push(this.menusApiService.removeMenuItem(menuId, current.itemId));
      }
    }

    if (operations.length === 0) {
      return new Observable<void>((subscriber) => {
        subscriber.next();
        subscriber.complete();
      });
    }

    return forkJoin(operations).pipe(map(() => void 0));
  }

  private mapMenuDrawerData(
    menu: MenuApiModel,
    menuItems: MenuItemApiModel[],
    availableItems: ItemApiModel[],
  ): Observable<AdminMenuDrawerData> {
    return new Observable<AdminMenuDrawerData>((subscriber) => {
      subscriber.next({
        availableItems,
        draft: {
          title: menu.title,
          description: menu.description ?? '',
          items: menuItems.map((item) => ({
            itemId: item.itemId,
            title: item.title,
            amount: String(item.amount),
          })),
        },
      });
      subscriber.complete();
    });
  }

  private clampPageIndex(): void {
    const maxPageIndex = Math.max(0, Math.ceil(this.rows().length / this.pageSize()) - 1);
    if (this.pageIndex() > maxPageIndex) {
      this.pageIndex.set(maxPageIndex);
    }
  }

  private mapLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);
    if (status === 401) {
      return 'Выполните вход, чтобы просматривать меню.';
    }

    return 'Не удалось загрузить список меню.';
  }

  private mapDrawerLoadErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Меню не найдено.'
      : 'Не удалось загрузить данные меню.';
  }

  private mapDrawerSaveErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 400
      ? 'Проверьте введенные данные меню.'
      : 'Не удалось сохранить меню.';
  }

  private mapDeleteErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Меню не найдено.'
      : 'Не удалось удалить меню.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
