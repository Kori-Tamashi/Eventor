/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { System_DateOnly } from '../models/System_DateOnly';
import type { Web_Dtos_CreateDayRequest } from '../models/Web_Dtos_CreateDayRequest';
import type { Web_Dtos_CreateEventRequest } from '../models/Web_Dtos_CreateEventRequest';
import type { Web_Dtos_UpdateEventRequest } from '../models/Web_Dtos_UpdateEventRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class EventsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Events({
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
            url: '/api/v1/events',
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
    public postApiV1Events({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateEventRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/events',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Events1({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/events/{eventId}',
            path: {
                'eventId': eventId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1Events({
        eventId,
        requestBody,
    }: {
        eventId: string,
        requestBody?: Web_Dtos_UpdateEventRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/events/{eventId}',
            path: {
                'eventId': eventId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1Events({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/events/{eventId}',
            path: {
                'eventId': eventId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EventsUser({
        userId,
        pageNumber,
        pageSize,
    }: {
        userId: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/events/user/{userId}',
            path: {
                'userId': userId,
            },
            query: {
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EventsOrganized({
        userId,
        pageNumber,
        pageSize,
    }: {
        userId: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/events/organized/{userId}',
            path: {
                'userId': userId,
            },
            query: {
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EventsDays({
        eventId,
        pageNumber,
        pageSize,
    }: {
        eventId: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/events/{eventId}/days',
            path: {
                'eventId': eventId,
            },
            query: {
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1EventsDays({
        eventId,
        requestBody,
    }: {
        eventId: string,
        requestBody?: Web_Dtos_CreateDayRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/events/{eventId}/days',
            path: {
                'eventId': eventId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
