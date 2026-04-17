import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { EconomyService } from '../generated';

@Injectable({
  providedIn: 'root',
})
export class EconomyApiService {
  private readonly economyService = inject(EconomyService);

  getDayPrice(dayId: string): Observable<number> {
    return this.economyService.getApiV1EconomyDaysPrice({ dayId }) as Observable<number>;
  }

  getDayPriceMap(dayIds: string[]): Observable<Record<string, number>> {
    const uniqueIds = Array.from(new Set(dayIds.filter(Boolean)));

    if (uniqueIds.length === 0) {
      return of({});
    }

    return forkJoin(
      uniqueIds.map((dayId) =>
        this.getDayPrice(dayId).pipe(
          map((price) => ({ id: dayId, price }))
        )
      )
    ).pipe(
      map((entries) =>
        entries.reduce<Record<string, number>>((acc, entry) => {
          acc[entry.id] = entry.price;
          return acc;
        }, {})
      )
    );
  }

  getDayPriceWithPrivileges(dayId: string): Observable<number> {
    return this.economyService.getApiV1EconomyDaysPriceWithPrivileges({ dayId }) as Observable<number>;
  }

  getMenuCost(menuId: string): Observable<number> {
    return this.economyService.getApiV1EconomyMenusCost({ menuId }) as Observable<number>;
  }

  getEventCost(eventId: string): Observable<number> {
    return this.economyService.getApiV1EconomyEventsCost({ eventId }) as Observable<number>;
  }

  getFundamentalPrice1D(eventId: string): Observable<number> {
    return this.economyService.getApiV1EconomyEventsFundamentalPrice1D({ eventId }) as Observable<number>;
  }

  getFundamentalPriceNd(eventId: string): Observable<number> {
    return this.economyService.getApiV1EconomyEventsFundamentalPriceNd({ eventId }) as Observable<number>;
  }

  getEventBalance1D(eventId: string): Observable<number> {
    return this.economyService.getApiV1EconomyEventsBalance1D({ eventId }) as Observable<number>;
  }

  getEventBalanceNd(eventId: string): Observable<number> {
    return this.economyService.getApiV1EconomyEventsBalanceNd({ eventId }) as Observable<number>;
  }
}
