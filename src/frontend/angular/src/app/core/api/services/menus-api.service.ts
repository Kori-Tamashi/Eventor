import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import type { Web_Dtos_UpdateMenuRequest } from '../generated';
import { AdminMenusService, MenusService } from '../generated';

export type MenuApiModel = {
  id: string;
  title: string;
  description?: string | null;
  menuItems: Array<{
    itemId: string;
    amount: number;
  }>;
};

export type MenuItemApiModel = {
  itemId: string;
  title: string;
  amount: number;
};

@Injectable({
  providedIn: 'root',
})
export class MenusApiService {
  private readonly menusService = inject(MenusService);
  private readonly adminMenusService = inject(AdminMenusService);

  listMenus(titleContains?: string, pageNumber?: number, pageSize?: number): Observable<MenuApiModel[]> {
    return this.menusService.getApiV1Menus({
      titleContains,
      pageNumber,
      pageSize,
    }) as Observable<MenuApiModel[]>;
  }

  getMenu(menuId: string): Observable<MenuApiModel> {
    return this.menusService.getApiV1Menus1({ menuId }) as Observable<MenuApiModel>;
  }

  getMenuTitleMap(menuIds: string[]): Observable<Record<string, string>> {
    const uniqueIds = Array.from(new Set(menuIds.filter(Boolean)));

    if (uniqueIds.length === 0) {
      return of({});
    }

    return forkJoin(
      uniqueIds.map((menuId) =>
        this.getMenu(menuId).pipe(
          map((menu) => ({ id: menuId, title: menu.title }))
        )
      )
    ).pipe(
      map((entries) =>
        entries.reduce<Record<string, string>>((acc, entry) => {
          acc[entry.id] = entry.title;
          return acc;
        }, {})
      )
    );
  }

  createMenu(title: string, description?: string): Observable<MenuApiModel> {
    return this.adminMenusService.postApiV1AdminMenus({
      requestBody: {
        title,
        description,
      },
    }) as Observable<MenuApiModel>;
  }

  listMenuItems(menuId: string, pageNumber?: number, pageSize?: number): Observable<MenuItemApiModel[]> {
    return this.menusService.getApiV1MenusItems({
      menuId,
      pageNumber,
      pageSize,
    }) as Observable<MenuItemApiModel[]>;
  }

  getMenuItemAmount(menuId: string, itemId: string): Observable<number> {
    return this.menusService.getApiV1MenusItemsAmount({
      menuId,
      itemId,
    }) as Observable<number>;
  }

  updateMenu(menuId: string, payload: Web_Dtos_UpdateMenuRequest): Observable<void> {
    return this.adminMenusService.putApiV1AdminMenus({
      menuId,
      requestBody: payload,
    }) as Observable<void>;
  }

  deleteMenu(menuId: string): Observable<void> {
    return this.adminMenusService.deleteApiV1AdminMenus({
      menuId,
    }) as Observable<void>;
  }

  addMenuItem(menuId: string, itemId: string, amount: number): Observable<void> {
    return this.adminMenusService.postApiV1AdminMenusItems({
      menuId,
      itemId,
      amount,
    }) as Observable<void>;
  }

  updateMenuItemAmount(menuId: string, itemId: string, amount: number): Observable<void> {
    return this.adminMenusService.putApiV1AdminMenusItems({
      menuId,
      itemId,
      amount,
    }) as Observable<void>;
  }

  removeMenuItem(menuId: string, itemId: string): Observable<void> {
    return this.adminMenusService.deleteApiV1AdminMenusItems({
      menuId,
      itemId,
    }) as Observable<void>;
  }
}
