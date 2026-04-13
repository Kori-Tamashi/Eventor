/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Domain_Enums_Gender } from '../models/Domain_Enums_Gender';
import type { Domain_Enums_UserRole } from '../models/Domain_Enums_UserRole';
import type { Web_Dtos_CreateUserRequest } from '../models/Web_Dtos_CreateUserRequest';
import type { Web_Dtos_UpdateUserRequest } from '../models/Web_Dtos_UpdateUserRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class AdminUsersService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1AdminUsers({
        nameContains,
        phone,
        role,
        gender,
        pageNumber,
        pageSize,
    }: {
        nameContains?: string,
        phone?: string,
        role?: Domain_Enums_UserRole,
        gender?: Domain_Enums_Gender,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/admin/users',
            query: {
                'NameContains': nameContains,
                'Phone': phone,
                'Role': role,
                'Gender': gender,
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1AdminUsers({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateUserRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/admin/users',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1AdminUsers1({
        userId,
    }: {
        userId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/admin/users/{userId}',
            path: {
                'userId': userId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1AdminUsers({
        userId,
        requestBody,
    }: {
        userId: string,
        requestBody?: Web_Dtos_UpdateUserRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/admin/users/{userId}',
            path: {
                'userId': userId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1AdminUsers({
        userId,
    }: {
        userId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/admin/users/{userId}',
            path: {
                'userId': userId,
            },
        });
    }
}
