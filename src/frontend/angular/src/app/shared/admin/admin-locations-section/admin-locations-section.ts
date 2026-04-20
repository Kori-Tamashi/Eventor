import { DestroyRef, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, Subscription, debounceTime, distinctUntilChanged, finalize } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { LocationApiModel } from '../../../core/api/models/location.models';
import { LocationsApiService } from '../../../core/api/services/locations-api.service';
import { AdminLocationDrawer } from '../admin-location-drawer/admin-location-drawer';
import {
  AdminLocationDraftValue,
  AdminLocationEditorMode,
  AdminLocationSavePayload,
} from '../admin-location-drawer/admin-location.models';
import { UiNotificationsService } from '../../../core/ui/ui-notifications.service';

@Component({
  selector: 'app-admin-locations-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, AdminLocationDrawer],
  templateUrl: './admin-locations-section.html',
  styleUrl: './admin-locations-section.scss',
})
export class AdminLocationsSection {
  private readonly destroyRef = inject(DestroyRef);
  private readonly locationsApiService = inject(LocationsApiService);
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
  readonly rows = signal<LocationApiModel[]>([]);

  readonly drawerOpen = signal<boolean>(false);
  readonly drawerMode = signal<AdminLocationEditorMode>('create');
  readonly drawerInitialValue = signal<AdminLocationDraftValue | null>(null);
  readonly drawerErrorMessage = signal<string>('');
  readonly isDrawerLoading = signal<boolean>(false);
  readonly isSavingDrawer = signal<boolean>(false);
  readonly editingLocationId = signal<string | null>(null);
  readonly deletingLocationId = signal<string | null>(null);

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

  readonly pageLabel = computed(() => {
    const { index } = this.paginationState();
    return `${index + 1} из ${this.totalPages()}`;
  });

  readonly pagedRows = computed(() => {
    const { all, size, index } = this.paginationState();
    const start = index * size;
    return all.slice(start, start + size);
  });

  readonly canPrev = computed(() => this.pageIndex() > 0);
  readonly canNext = computed(() => this.pageIndex() < this.totalPages() - 1);

  constructor() {
    this.searchInput$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((value) => {
        this.pageIndex.set(0);
        this.loadLocations(value);
      });

    this.loadLocations('');
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
    this.editingLocationId.set(null);
    this.drawerInitialValue.set({
      title: '',
      description: '',
      cost: '',
      capacity: '',
    });
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.drawerOpen.set(true);
  }

  openEditDrawer(row: LocationApiModel): void {
    this.drawerMode.set('edit');
    this.editingLocationId.set(row.id);
    this.drawerInitialValue.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(true);
    this.drawerOpen.set(true);

    this.drawerLoadSubscription?.unsubscribe();
    this.drawerLoadSubscription = this.locationsApiService.getLocation(row.id)
      .pipe(finalize(() => this.isDrawerLoading.set(false)))
      .subscribe({
        next: (location) => {
          this.drawerInitialValue.set(this.mapLocationToDraft(location));
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
    this.editingLocationId.set(null);
  }

  saveLocation(payload: AdminLocationSavePayload): void {
    this.drawerErrorMessage.set('');
    this.isSavingDrawer.set(true);
    this.saveSubscription?.unsubscribe();

    let request$: Observable<unknown>;

    if (this.drawerMode() === 'edit' && this.editingLocationId()) {
      request$ = this.locationsApiService.updateLocation(this.editingLocationId()!, payload);
    } else {
      request$ = this.locationsApiService.createLocation(payload);
    }

    this.saveSubscription = request$
      .pipe(finalize(() => this.isSavingDrawer.set(false)))
      .subscribe({
        next: () => {
          const mode = this.drawerMode();
          this.closeDrawer();
          this.uiNotificationsService.success(mode === 'edit' ? 'Локация обновлена.' : 'Локация создана.');
          this.loadLocations(this.searchTerm().trim());
        },
        error: (error: unknown) => {
          this.drawerErrorMessage.set(this.mapDrawerSaveErrorMessage(error));
        },
      });
  }

  deleteLocation(row: LocationApiModel): void {
    if (this.deletingLocationId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm(`Удалить локацию "${row.title}"?`)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.deletingLocationId.set(row.id);
    this.deleteSubscription?.unsubscribe();
    this.deleteSubscription = this.locationsApiService.deleteLocation(row.id)
      .pipe(finalize(() => this.deletingLocationId.set(null)))
      .subscribe({
        next: () => {
          this.rows.update((rows) => rows.filter((item) => item.id !== row.id));
          this.clampPageIndex();
          this.uiNotificationsService.success('Локация удалена.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteErrorMessage(error));
        },
      });
  }

  private loadLocations(titleContains: string): void {
    this.loadSubscription?.unsubscribe();
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.loadSubscription = this.locationsApiService.listLocations(titleContains || undefined)
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
    const size = this.pageSize();
    const maxPageIndex = Math.max(0, Math.ceil(this.rows().length / size) - 1);

    if (this.pageIndex() > maxPageIndex) {
      this.pageIndex.set(maxPageIndex);
    }
  }

  private mapLocationToDraft(location: LocationApiModel): AdminLocationDraftValue {
    return {
      title: location.title,
      description: location.description ?? '',
      cost: this.formatNumber(location.cost),
      capacity: String(location.capacity),
    };
  }

  private formatNumber(value: number): string {
    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }

  private mapLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы просматривать локации.';
    }

    return 'Не удалось загрузить список локаций.';
  }

  private mapDrawerLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 404) {
      return 'Локация не найдена.';
    }

    return 'Не удалось загрузить данные локации.';
  }

  private mapDrawerSaveErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 400) {
      return 'Проверьте введенные данные локации.';
    }

    return 'Не удалось сохранить локацию.';
  }

  private mapDeleteErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 404) {
      return 'Локация не найдена.';
    }

    return 'Не удалось удалить локацию.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
