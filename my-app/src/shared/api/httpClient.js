import { API_URL } from './baseUrl';

// Ключи авторизации в localStorage. При 401 удаляем только их —
// раньше вызывался localStorage.clear(), который стирал и кэш каталога, и прочие настройки.
const AUTH_KEYS = ['token', 'userId', 'userName', 'userRole', 'needRefreshProfile'];

const PUBLIC_PATHS = ['/SignIn', '/SignUp', '/'];

/**
 * Достаёт из тела ответа текст ошибки сервера ({ error } / { message }).
 * Тело может быть не-JSON — тогда возвращает null.
 */
const readErrorBody = async (response) => {
    try {
        // clone(): тело исходного ответа остаётся непрочитанным для кода,
        // который сам вызывает error.response.json()
        const source = typeof response.clone === 'function' ? response.clone() : response;
        if (typeof source.json !== 'function') return null;
        return await source.json();
    } catch {
        return null;
    }
};

const pickMessage = (body) => {
    if (!body || typeof body !== 'object') return null;
    return body.error || body.message || body.Error || body.Message || body.title || null;
};

export const http = async (endpoint, options = {}) => {
    const token = localStorage.getItem('token');

    // Content-Type только для запросов с телом: GET с этим заголовком браузер считает
    // «непростым» и перед каждым запросом к API шлёт дополнительный CORS-preflight (OPTIONS).
    const headers = {
        ...(options.body !== undefined && { 'Content-Type': 'application/json' }),
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...(options.headers || {})
    };

    const url = endpoint.startsWith('http') ? endpoint : `${API_URL}${endpoint}`;

    const response = await fetch(url, { ...options, headers });

    if (response.status === 401) {
        const currentPath = window.location.pathname;
        if (!PUBLIC_PATHS.includes(currentPath)) {
            AUTH_KEYS.forEach(key => localStorage.removeItem(key));
            window.location.href = '/SignIn';
        }
        const error = new Error('Unauthorized');
        error.status = 401;
        error.response = response;
        throw error;
    }

    if (!response.ok) {
        const body = await readErrorBody(response);
        const error = new Error(pickMessage(body) || `HTTP ${response.status}`);
        error.status = response.status;
        error.response = response;
        error.data = body;
        // 409 — данные изменились параллельно (например, товар раскупили), можно повторить
        error.isConflict = response.status === 409;
        throw error;
    }

    return response;
};

http.get = (endpoint, options = {}) => http(endpoint, { ...options, method: 'GET' });
http.post = (endpoint, body, options = {}) => http(endpoint, { ...options, method: 'POST', body: JSON.stringify(body) });
http.put = (endpoint, body, options = {}) => http(endpoint, { ...options, method: 'PUT', body: body === undefined ? undefined : JSON.stringify(body) });
http.delete = (endpoint, options = {}) => http(endpoint, { ...options, method: 'DELETE' });

export default http;
