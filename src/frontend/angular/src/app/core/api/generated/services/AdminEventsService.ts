/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { System_DateOnly } from '../models/System_DateOnly';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class AdminEventsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1AdminEvents({
        locationId,
        startDateFrom,
        startDateTo,
        titleContains,
        pageNumber,
        pageSize,
    }: {
        locationId?: string,
        startDateFrom?: System_DateOnly,
        startDateTo?: System_DateOnly,
        titleContains?: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/admin/events',
            query: {
                'LocationId': locationId,
                'StartDateFrom': startDateFrom,
                'StartDateTo': startDateTo,
                'TitleContains': titleContains,
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1AdminEvents({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/admin/events/{eventId}',
            path: {
                'eventId': eventId,
            },
        });
    }
}
