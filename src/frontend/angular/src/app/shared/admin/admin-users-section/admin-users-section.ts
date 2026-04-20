import { DestroyRef, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, Subscription, debounceTime, distinctUntilChanged, finalize } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { Web_Dtos_Gender, Web_Dtos_UserRole } from '../../../core/api/generated';
import { AdminUsersApiService } from '../../../core/api/services/admin-users-api.service';
import { CurrentUser } from '../../../core/api/models/user.models';
import { AdminUserDrawer } from '../admin-user-drawer/admin-user-drawer';
import { AdminUserDraftValue, AdminUserEditorMode, AdminUserSavePayload } from '../admin-user-drawer/admin-user.models';

@Component({
  selector: 'app-admin-users-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, AdminUserDrawer],
  templateUrl: './admin-users-section.html',
  styleUrl: './admin-users-section.scss',
})
export class AdminUsersSection {
  private readonly destroyRef = inject(DestroyRef);
  private readonly adminUsersApiService = inject(AdminUsersApiService);

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
  readonly rows = signal<CurrentUser[]>([]);

  readonly drawerOpen = signal<boolean>(false);
  readonly drawerMode = signal<AdminUserEditorMode>('create');
  readonly drawerInitialValue = signal<AdminUserDraftValue | null>(null);
  readonly drawerErrorMessage = signal<string>('');
  readonly isDrawerLoading = signal<boolean>(false);
  readonly isSavingDrawer = signal<boolean>(false);
  readonly editingUserId = signal<string | null>(null);
  readonly deletingUserId = signal<string | null>(null);

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
        this.loadUsers(value);
      });

    this.loadUsers('');
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
    this.editingUserId.set(null);
    this.drawerInitialValue.set({
      name: '',
      phone: '',
      gender: 0,
      role: 2,
      password: '',
    });
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.drawerOpen.set(true);
  }

  openEditDrawer(row: CurrentUser): void {
    this.drawerMode.set('edit');
    this.editingUserId.set(row.id);
    this.drawerInitialValue.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(true);
    this.drawerOpen.set(true);

    this.drawerLoadSubscription?.unsubscribe();
    this.drawerLoadSubscription = this.adminUsersApiService.getUser(row.id)
      .pipe(finalize(() => this.isDrawerLoading.set(false)))
      .subscribe({
        next: (user) => {
          this.drawerInitialValue.set({
            name: user.name,
            phone: user.phone,
            gender: user.gender,
            role: user.role,
            password: '',
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
    this.drawerInitialValue.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.isSavingDrawer.set(false);
    this.editingUserId.set(null);
  }

  saveUser(payload: AdminUserSavePayload): void {
    this.drawerErrorMessage.set('');
    this.isSavingDrawer.set(true);
    this.saveSubscription?.unsubscribe();

    let request$: Observable<unknown>;
    if (this.drawerMode() === 'edit' && this.editingUserId()) {
      request$ = this.adminUsersApiService.updateUser(this.editingUserId()!, payload);
    } else {
      request$ = this.adminUsersApiService.createUser({
        name: payload.name,
        phone: payload.phone,
        gender: payload.gender,
        role: payload.role,
        password: payload.password ?? '',
      });
    }

    this.saveSubscription = request$
      .pipe(finalize(() => this.isSavingDrawer.set(false)))
      .subscribe({
        next: () => {
          const mode = this.drawerMode();
          this.closeDrawer();
          this.successMessage.set(mode === 'edit' ? 'Пользователь обновлен.' : 'Пользователь создан.');
          this.loadUsers(this.searchTerm().trim());
        },
        error: (error: unknown) => {
          this.drawerErrorMessage.set(this.mapDrawerSaveErrorMessage(error));
        },
      });
  }

  deleteUser(row: CurrentUser): void {
    if (this.deletingUserId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm(`Удалить пользователя "${row.name}"?`)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.deletingUserId.set(row.id);
    this.deleteSubscription?.unsubscribe();
    this.deleteSubscription = this.adminUsersApiService.deleteUser(row.id)
      .pipe(finalize(() => this.deletingUserId.set(null)))
      .subscribe({
        next: () => {
          this.rows.update((rows) => rows.filter((user) => user.id !== row.id));
          this.clampPageIndex();
          this.successMessage.set('Пользователь удален.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteErrorMessage(error));
        },
      });
  }

  roleLabel(role: Web_Dtos_UserRole): string {
    switch (role) {
      case 0: return 'Администратор';
      case 1: return 'Пользователь';
      case 2: return 'Гость';
      default: return 'Не указано';
    }
  }

  genderLabel(gender: Web_Dtos_Gender): string {
    switch (gender) {
      case 0: return 'Мужской';
      case 1: return 'Женский';
      default: return 'Не указано';
    }
  }

  private loadUsers(nameContains: string): void {
    this.loadSubscription?.unsubscribe();
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.loadSubscription = this.adminUsersApiService.listUsers({
      nameContains: nameContains || undefined,
    })
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

  private mapLoadErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 401
      ? 'Выполните вход, чтобы просматривать пользователей.'
      : 'Не удалось загрузить список пользователей.';
  }

  private mapDrawerLoadErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Пользователь не найден.'
      : 'Не удалось загрузить данные пользователя.';
  }

  private mapDrawerSaveErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 400
      ? 'Проверьте введенные данные пользователя.'
      : 'Не удалось сохранить пользователя.';
  }

  private mapDeleteErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Пользователь не найден.'
      : 'Не удалось удалить пользователя.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
