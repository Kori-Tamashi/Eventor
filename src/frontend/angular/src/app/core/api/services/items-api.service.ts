import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ItemsService } from '../generated';

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
}
