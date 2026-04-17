import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
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

  getMenu(menuId: string): Observable<MenuApiModel> {
    return this.menusService.getApiV1Menus1({ menuId }) as Observable<MenuApiModel>;
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
