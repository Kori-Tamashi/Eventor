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
}
