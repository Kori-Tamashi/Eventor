/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Domain_Enums_RegistrationType } from '../models/Domain_Enums_RegistrationType';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class AdminRegistrationsService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1AdminRegistrations({
        eventId,
        userId,
        type,
        payment,
        pageNumber,
        pageSize,
    }: {
        eventId?: string,
        userId?: string,
        type?: Domain_Enums_RegistrationType,
        payment?: boolean,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/admin/registrations',
            query: {
                'EventId': eventId,
                'UserId': userId,
                'Type': type,
                'Payment': payment,
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
}
