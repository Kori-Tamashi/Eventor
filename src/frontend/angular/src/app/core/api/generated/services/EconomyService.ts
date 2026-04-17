/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class EconomyService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyItemsCost({
        itemId,
    }: {
        itemId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/items/{itemId}/cost',
            path: {
                'itemId': itemId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyMenusCost({
        menuId,
    }: {
        menuId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/menus/{menuId}/cost',
            path: {
                'menuId': menuId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1EconomyDaysCost({
        requestBody,
    }: {
        requestBody?: Array<string>,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/economy/days/cost',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyEventsCost({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/events/{eventId}/cost',
            path: {
                'eventId': eventId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyDaysPrice({
        dayId,
    }: {
        dayId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/days/{dayId}/price',
            path: {
                'dayId': dayId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyDaysPriceWithPrivileges({
        dayId,
    }: {
        dayId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/days/{dayId}/price/with-privileges',
            path: {
                'dayId': dayId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1EconomyDaysPrice({
        requestBody,
    }: {
        requestBody?: Array<string>,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/economy/days/price',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyEventsFundamentalPrice1D({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/events/{eventId}/fundamental-price/1d',
            path: {
                'eventId': eventId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyEventsFundamentalPriceNd({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/events/{eventId}/fundamental-price/nd',
            path: {
                'eventId': eventId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyEventsBalance1D({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/events/{eventId}/balance/1d',
            path: {
                'eventId': eventId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1EconomyEventsBalanceNd({
        eventId,
    }: {
        eventId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/economy/events/{eventId}/balance/nd',
            path: {
                'eventId': eventId,
            },
        });
    }
}
