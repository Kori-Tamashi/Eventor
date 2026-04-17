/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Web_Dtos_CreateMenuRequest } from '../models/Web_Dtos_CreateMenuRequest';
import type { Web_Dtos_UpdateMenuRequest } from '../models/Web_Dtos_UpdateMenuRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class AdminMenusService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1AdminMenus({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateMenuRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/admin/menus',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1AdminMenus({
        menuId,
        requestBody,
    }: {
        menuId: string,
        requestBody?: Web_Dtos_UpdateMenuRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/admin/menus/{menuId}',
            path: {
                'menuId': menuId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1AdminMenus({
        menuId,
    }: {
        menuId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/admin/menus/{menuId}',
            path: {
                'menuId': menuId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1AdminMenusItems({
        menuId,
        itemId,
        amount,
    }: {
        menuId: string,
        itemId: string,
        amount?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/admin/menus/{menuId}/items/{itemId}',
            path: {
                'menuId': menuId,
                'itemId': itemId,
            },
            query: {
                'amount': amount,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1AdminMenusItems({
        menuId,
        itemId,
        amount,
    }: {
        menuId: string,
        itemId: string,
        amount?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/admin/menus/{menuId}/items/{itemId}',
            path: {
                'menuId': menuId,
                'itemId': itemId,
            },
            query: {
                'amount': amount,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1AdminMenusItems({
        menuId,
        itemId,
    }: {
        menuId: string,
        itemId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/admin/menus/{menuId}/items/{itemId}',
            path: {
                'menuId': menuId,
                'itemId': itemId,
            },
        });
    }
}
