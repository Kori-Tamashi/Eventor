import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { LocationsService } from '../generated';
import { LocationApiModel } from '../models/location.models';

@Injectable({
  providedIn: 'root',
})
export class LocationsApiService {
  private readonly locationsService = inject(LocationsService);

  getLocation(locationId: string): Observable<LocationApiModel> {
    return this.locationsService.getApiV1Locations1({ locationId }) as Observable<LocationApiModel>;
  }

  getLocationTitleMap(locationIds: string[]): Observable<Record<string, string>> {
    const uniqueIds = Array.from(new Set(locationIds.filter(Boolean)));

    if (uniqueIds.length === 0) {
      return of({});
    }

    return forkJoin(
      uniqueIds.map((locationId) =>
        this.getLocation(locationId).pipe(
          map((location) => ({ id: locationId, title: location.title }))
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
}
