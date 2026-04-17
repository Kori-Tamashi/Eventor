/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Web_Dtos_UpdateDayRequest } from '../models/Web_Dtos_UpdateDayRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class DaysService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Days({
        eventId,
        menuId,
        pageNumber,
        pageSize,
    }: {
        eventId?: string,
        menuId?: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/days',
            query: {
                'EventId': eventId,
                'MenuId': menuId,
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Days1({
        dayId,
    }: {
        dayId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/days/{dayId}',
            path: {
                'dayId': dayId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1Days({
        dayId,
        requestBody,
    }: {
        dayId: string,
        requestBody?: Web_Dtos_UpdateDayRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/days/{dayId}',
            path: {
                'dayId': dayId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1Days({
        dayId,
    }: {
        dayId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/days/{dayId}',
            path: {
                'dayId': dayId,
            },
        });
    }
}
