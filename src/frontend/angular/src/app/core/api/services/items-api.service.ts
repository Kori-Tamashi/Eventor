import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type {
  Web_Dtos_CreateItemRequest,
  Web_Dtos_UpdateItemRequest,
} from '../generated';
import { AdminItemsService, ItemsService } from '../generated';

export type ItemApiModel = {
  id: string;
  title: string;
  cost: number;
};

@Injectable({
  providedIn: 'root',
})
export class ItemsApiService {
  private readonly itemsService = inject(ItemsService);
  private readonly adminItemsService = inject(AdminItemsService);

  listItems(titleContains?: string, pageNumber?: number, pageSize?: number): Observable<ItemApiModel[]> {
    return this.itemsService.getApiV1Items({
      titleContains,
      pageNumber,
      pageSize,
    }) as Observable<ItemApiModel[]>;
  }

  getItem(itemId: string): Observable<ItemApiModel> {
    return this.itemsService.getApiV1Items1({ itemId }) as Observable<ItemApiModel>;
  }

  createItem(payload: Web_Dtos_CreateItemRequest): Observable<ItemApiModel> {
    return this.adminItemsService.postApiV1AdminItems({
      requestBody: payload,
    }) as Observable<ItemApiModel>;
  }

  updateItem(itemId: string, payload: Web_Dtos_UpdateItemRequest): Observable<void> {
    return this.adminItemsService.putApiV1AdminItems({
      itemId,
      requestBody: payload,
    }) as Observable<void>;
  }

  deleteItem(itemId: string): Observable<void> {
    return this.adminItemsService.deleteApiV1AdminItems({
      itemId,
    }) as Observable<void>;
  }
}
