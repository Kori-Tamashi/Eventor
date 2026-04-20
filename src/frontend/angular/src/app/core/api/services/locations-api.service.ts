import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import type {
  Web_Dtos_CreateLocationRequest,
  Web_Dtos_UpdateLocationRequest,
} from '../generated';
import { AdminLocationsService, LocationsService } from '../generated';
import { LocationApiModel } from '../models/location.models';

@Injectable({
  providedIn: 'root',
})
export class LocationsApiService {
  private readonly locationsService = inject(LocationsService);
  private readonly adminLocationsService = inject(AdminLocationsService);

  listLocations(titleContains?: string): Observable<LocationApiModel[]> {
    return this.locationsService.getApiV1Locations({
      titleContains,
    }) as Observable<LocationApiModel[]>;
  }

  getLocation(locationId: string): Observable<LocationApiModel> {
    return this.locationsService.getApiV1Locations1({ locationId }) as Observable<LocationApiModel>;
  }

  createLocation(payload: Web_Dtos_CreateLocationRequest): Observable<LocationApiModel> {
    return this.adminLocationsService.postApiV1AdminLocations({
      requestBody: payload,
    }) as Observable<LocationApiModel>;
  }

  updateLocation(locationId: string, payload: Web_Dtos_UpdateLocationRequest): Observable<void> {
    return this.adminLocationsService.putApiV1AdminLocations({
      locationId,
      requestBody: payload,
    }) as Observable<void>;
  }

  deleteLocation(locationId: string): Observable<void> {
    return this.adminLocationsService.deleteApiV1AdminLocations({
      locationId,
    }) as Observable<void>;
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
