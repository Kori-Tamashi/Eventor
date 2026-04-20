import { DestroyRef, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, Subscription, debounceTime, distinctUntilChanged, finalize, forkJoin } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { EconomyApiService } from '../../../core/api/services/economy-api.service';
import { ItemApiModel, ItemsApiService } from '../../../core/api/services/items-api.service';
import { AdminItemDrawer } from '../admin-item-drawer/admin-item-drawer';
import { AdminItemDraftValue, AdminItemEditorMode, AdminItemSavePayload } from '../admin-item-drawer/admin-item.models';
import { UiNotificationsService } from '../../../core/ui/ui-notifications.service';

@Component({
  selector: 'app-admin-items-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, AdminItemDrawer],
  templateUrl: './admin-items-section.html',
  styleUrl: './admin-items-section.scss',
})
export class AdminItemsSection {
  private readonly destroyRef = inject(DestroyRef);
  private readonly itemsApiService = inject(ItemsApiService);
  private readonly economyApiService = inject(EconomyApiService);
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
  readonly rows = signal<ItemApiModel[]>([]);

  readonly drawerOpen = signal<boolean>(false);
  readonly drawerMode = signal<AdminItemEditorMode>('create');
  readonly drawerInitialValue = signal<AdminItemDraftValue | null>(null);
  readonly drawerErrorMessage = signal<string>('');
  readonly isDrawerLoading = signal<boolean>(false);
  readonly isSavingDrawer = signal<boolean>(false);
  readonly editingItemId = signal<string | null>(null);
  readonly deletingItemId = signal<string | null>(null);

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
        this.loadItems(value);
      });

    this.loadItems('');
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
    this.editingItemId.set(null);
    this.drawerInitialValue.set({ title: '', cost: '' });
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.drawerOpen.set(true);
  }

  openEditDrawer(row: ItemApiModel): void {
    this.drawerMode.set('edit');
    this.editingItemId.set(row.id);
    this.drawerInitialValue.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(true);
    this.drawerOpen.set(true);

    this.drawerLoadSubscription?.unsubscribe();
    this.drawerLoadSubscription = forkJoin({
      item: this.itemsApiService.getItem(row.id),
      calculatedCost: this.economyApiService.getItemCost(row.id),
    })
      .pipe(finalize(() => this.isDrawerLoading.set(false)))
      .subscribe({
        next: ({ item, calculatedCost }) => {
          this.drawerInitialValue.set({
            title: item.title,
            cost: this.formatNumber(item.cost),
            calculatedCost: this.formatNumber(calculatedCost),
          });
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
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.isSavingDrawer.set(false);
    this.editingItemId.set(null);
  }

  saveItem(payload: AdminItemSavePayload): void {
    this.drawerErrorMessage.set('');
    this.isSavingDrawer.set(true);
    this.saveSubscription?.unsubscribe();

    let request$: Observable<unknown>;
    if (this.drawerMode() === 'edit' && this.editingItemId()) {
      request$ = this.itemsApiService.updateItem(this.editingItemId()!, payload);
    } else {
      request$ = this.itemsApiService.createItem(payload);
    }

    this.saveSubscription = request$
      .pipe(finalize(() => this.isSavingDrawer.set(false)))
      .subscribe({
        next: () => {
          const mode = this.drawerMode();
          this.closeDrawer();
          this.uiNotificationsService.success(mode === 'edit' ? 'Объект обновлен.' : 'Объект создан.');
          this.loadItems(this.searchTerm().trim());
        },
        error: (error: unknown) => {
          this.drawerErrorMessage.set(this.mapDrawerSaveErrorMessage(error));
        },
      });
  }

  deleteItem(row: ItemApiModel): void {
    if (this.deletingItemId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm(`Удалить объект "${row.title}"?`)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.deletingItemId.set(row.id);
    this.deleteSubscription?.unsubscribe();
    this.deleteSubscription = this.itemsApiService.deleteItem(row.id)
      .pipe(finalize(() => this.deletingItemId.set(null)))
      .subscribe({
        next: () => {
          this.rows.update((rows) => rows.filter((item) => item.id !== row.id));
          this.clampPageIndex();
          this.uiNotificationsService.success('Объект удален.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteErrorMessage(error));
        },
      });
  }

  private loadItems(titleContains: string): void {
    this.loadSubscription?.unsubscribe();
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.loadSubscription = this.itemsApiService.listItems(titleContains || undefined)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (rows) => {
          this.rows.set(rows);
          this.clampPageIndex();
        },
        error: (error: unknown) => {
          this.rows.set([]);
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  private clampPageIndex(): void {
    const maxPageIndex = Math.max(0, Math.ceil(this.rows().length / this.pageSize()) - 1);
    if (this.pageIndex() > maxPageIndex) {
      this.pageIndex.set(maxPageIndex);
    }
  }

  private formatNumber(value: number): string {
    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }

  private mapLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);
    if (status === 401) {
      return 'Выполните вход, чтобы просматривать объекты.';
    }

    return 'Не удалось загрузить список объектов.';
  }

  private mapDrawerLoadErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Объект не найден.'
      : 'Не удалось загрузить данные объекта.';
  }

  private mapDrawerSaveErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 400
      ? 'Проверьте введенные данные объекта.'
      : 'Не удалось сохранить объект.';
  }

  private mapDeleteErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Объект не найден.'
      : 'Не удалось удалить объект.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
