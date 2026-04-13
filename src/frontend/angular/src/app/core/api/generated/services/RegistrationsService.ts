/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Web_Dtos_CreateRegistrationRequest } from '../models/Web_Dtos_CreateRegistrationRequest';
import type { Web_Dtos_UpdateRegistrationRequest } from '../models/Web_Dtos_UpdateRegistrationRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class RegistrationsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1Registrations({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateRegistrationRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/registrations',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1RegistrationsUser({
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
            url: '/api/v1/registrations/user/{userId}',
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
    public getApiV1Registrations({
        registrationId,
    }: {
        registrationId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/registrations/{registrationId}',
            path: {
                'registrationId': registrationId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1Registrations({
        registrationId,
        requestBody,
    }: {
        registrationId: string,
        requestBody?: Web_Dtos_UpdateRegistrationRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/registrations/{registrationId}',
            path: {
                'registrationId': registrationId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1Registrations({
        registrationId,
    }: {
        registrationId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/registrations/{registrationId}',
            path: {
                'registrationId': registrationId,
            },
        });
    }
}
