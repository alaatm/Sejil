// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

// Credits to https://github.com/aspnet/SignalR/blob/dev/client-ts/Microsoft.AspNetCore.SignalR.Client.TS/HttpClient.ts

export interface IHttpClient {
    get(url: string): Promise<string>;
    post(url: string, content: string): Promise<string>;
}

export class HttpClient implements IHttpClient {
    get(url: string): Promise<string> {
        return this.xhr('GET', url);
    }

    post(url: string, content: string = ''): Promise<string> {
        return this.xhr('POST', url, content);
    }

    private xhr(method: string, url: string, content?: string): Promise<string> {
        return new Promise<string>((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open(method, url, true);

            if (method === 'POST' && content != null) {
                xhr.setRequestHeader('Content-type', 'application/json');
            }

            xhr.send(content);
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.response);
                } else {
                    reject({
                        status: xhr.status,
                        statusText: xhr.statusText,
                        data: xhr.response,
                    });
                }
            };

            xhr.onerror = () => {
                reject({
                    status: xhr.status,
                    statusText: xhr.statusText,
                });
            };
        });
    }
}
