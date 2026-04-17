/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Web_Dtos_CreateItemRequest } from '../models/Web_Dtos_CreateItemRequest';
import type { Web_Dtos_UpdateItemRequest } from '../models/Web_Dtos_UpdateItemRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class AdminItemsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1AdminItems({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateItemRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/admin/items',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1AdminItems({
        itemId,
        requestBody,
    }: {
        itemId: string,
        requestBody?: Web_Dtos_UpdateItemRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/admin/items/{itemId}',
            path: {
                'itemId': itemId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1AdminItems({
        itemId,
    }: {
        itemId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/admin/items/{itemId}',
            path: {
                'itemId': itemId,
            },
        });
    }
}
