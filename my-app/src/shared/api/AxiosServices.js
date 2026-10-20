import { http } from './httpClient';

const api = {
    get: (endpoint) => http.get(endpoint),
    post: (endpoint, data) => http.post(endpoint, data),
    put: (endpoint, data) => http.put(endpoint, data),
    delete: (endpoint) => http.delete(endpoint)
};

export default api;