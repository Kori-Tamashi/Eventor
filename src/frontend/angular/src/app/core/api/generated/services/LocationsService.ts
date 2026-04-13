/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Web_Dtos_CreateLocationRequest } from '../models/Web_Dtos_CreateLocationRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class LocationsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Locations({
        titleContains,
        costFrom,
        costTo,
        capacityFrom,
        capacityTo,
        pageNumber,
        pageSize,
    }: {
        titleContains?: string,
        costFrom?: number,
        costTo?: number,
        capacityFrom?: number,
        capacityTo?: number,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/locations',
            query: {
                'TitleContains': titleContains,
                'CostFrom': costFrom,
                'CostTo': costTo,
                'CapacityFrom': capacityFrom,
                'CapacityTo': capacityTo,
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1Locations({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateLocationRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/locations',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Locations1({
        locationId,
    }: {
        locationId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/locations/{locationId}',
            path: {
                'locationId': locationId,
            },
        });
    }
}
