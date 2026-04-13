/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { Domain_Enums_FeedbackSortByRate } from '../models/Domain_Enums_FeedbackSortByRate';
import type { Web_Dtos_CreateFeedbackRequest } from '../models/Web_Dtos_CreateFeedbackRequest';
import type { Web_Dtos_UpdateFeedbackRequest } from '../models/Web_Dtos_UpdateFeedbackRequest';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
@Injectable({
    providedIn: 'root',
})
export class FeedbacksService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Feedbacks({
        registrationId,
        sortByRate,
        pageNumber,
        pageSize,
    }: {
        registrationId?: string,
        sortByRate?: Domain_Enums_FeedbackSortByRate,
        pageNumber?: number,
        pageSize?: number,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/feedbacks',
            query: {
                'RegistrationId': registrationId,
                'SortByRate': sortByRate,
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public postApiV1Feedbacks({
        requestBody,
    }: {
        requestBody?: Web_Dtos_CreateFeedbackRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/api/v1/feedbacks',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1Feedbacks1({
        feedbackId,
    }: {
        feedbackId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/api/v1/feedbacks/{feedbackId}',
            path: {
                'feedbackId': feedbackId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public putApiV1Feedbacks({
        feedbackId,
        requestBody,
    }: {
        feedbackId: string,
        requestBody?: Web_Dtos_UpdateFeedbackRequest,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/api/v1/feedbacks/{feedbackId}',
            path: {
                'feedbackId': feedbackId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public deleteApiV1Feedbacks({
        feedbackId,
    }: {
        feedbackId: string,
    }): Observable<any> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/api/v1/feedbacks/{feedbackId}',
            path: {
                'feedbackId': feedbackId,
            },
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public getApiV1FeedbacksEvent({
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
            url: '/api/v1/feedbacks/event/{eventId}',
            path: {
                'eventId': eventId,
            },
            query: {
                'PageNumber': pageNumber,
                'PageSize': pageSize,
            },
        });
    }
}
