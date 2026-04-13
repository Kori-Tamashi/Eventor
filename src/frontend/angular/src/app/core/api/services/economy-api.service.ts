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
}
