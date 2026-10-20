import { API_URL } from './baseUrl';

export const http = async (endpoint, options = {}) => {
    const token = localStorage.getItem('token');

    const headers = {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...(options.headers || {})
    };

    const url = endpoint.startsWith('http') ? endpoint : `${API_URL}${endpoint}`;

    const response = await fetch(url, { ...options, headers });

    if (response.status === 401) {
        const currentPath = window.location.pathname;
        if (currentPath !== '/SignIn' && currentPath !== '/SignUp' && currentPath !== '/') {
            localStorage.clear();
            window.location.href = '/SignIn';
        }
        const error = new Error('Unauthorized');
        error.status = 401;
        error.response = response;
        throw error;
    }

    if (!response.ok) {
        const error = new Error(`HTTP ${response.status}`);
        error.status = response.status;
        error.response = response;
        throw error;
    }

    return response;
};

http.get = (endpoint, options = {}) => http(endpoint, { ...options, method: 'GET' });
http.post = (endpoint, body, options = {}) => http(endpoint, { ...options, method: 'POST', body: JSON.stringify(body) });
http.put = (endpoint, body, options = {}) => http(endpoint, { ...options, method: 'PUT', body: JSON.stringify(body) });
http.delete = (endpoint, options = {}) => http(endpoint, { ...options, method: 'DELETE' });

export default http;