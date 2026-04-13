/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Web_Dtos_CreateLocationRequest } from '../models/Web_Dtos_CreateLocationRequest';
import type { Web_Dtos_UpdateLocationRequest } from '../models/Web_Dtos_UpdateLocationRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class AdminLocationsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1AdminLocations({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateLocationRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/admin/locations',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1AdminLocations({
        locationId,
        requestBody,
    }: {
        locationId: string,
        requestBody?: Web_Dtos_UpdateLocationRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/admin/locations/{locationId}',
            path: {
                'locationId': locationId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1AdminLocations({
        locationId,
    }: {
        locationId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/admin/locations/{locationId}',
            path: {
                'locationId': locationId,
            },
        });
    }
}
