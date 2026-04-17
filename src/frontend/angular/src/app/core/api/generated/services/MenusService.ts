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
export class MenusService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Menus({
        titleContains,
        pageNumber,
        pageSize,
    }: {
        titleContains?: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/menus',
            query: {
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
    public getApiV1Menus1({
        menuId,
    }: {
        menuId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/menus/{menuId}',
            path: {
                'menuId': menuId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1MenusItems({
        menuId,
        pageNumber,
        pageSize,
    }: {
        menuId: string,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/menus/{menuId}/items',
            path: {
                'menuId': menuId,
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
    public getApiV1MenusItemsAmount({
        menuId,
        itemId,
    }: {
        menuId: string,
        itemId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/menus/{menuId}/items/{itemId}/amount',
            path: {
                'menuId': menuId,
                'itemId': itemId,
            },
        });
    }
}
