import api from './AxiosServices';
import { http } from './httpClient';

jest.mock('./httpClient', () => ({
    http: {
        get: jest.fn(),
        post: jest.fn(),
        put: jest.fn(),
        delete: jest.fn()
    }
}));

describe('AxiosServices (api)', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('api.get вызывает http.get', () => {
        api.get('/test');
        expect(http.get).toHaveBeenCalledWith('/test');
    });

    test('api.post вызывает http.post с endpoint и data', () => {
        api.post('/test', { name: 'test' });
        expect(http.post).toHaveBeenCalledWith('/test', { name: 'test' });
    });

    test('api.put вызывает http.put с endpoint и data', () => {
        api.put('/test/1', { name: 'updated' });
        expect(http.put).toHaveBeenCalledWith('/test/1', { name: 'updated' });
    });

    test('api.delete вызывает http.delete', () => {
        api.delete('/test/1');
        expect(http.delete).toHaveBeenCalledWith('/test/1');
    });

    test('api.get возвращает thenable-объект', () => {
        http.get.mockResolvedValueOnce({ ok: true });
        const result = api.get('/test');
        expect(typeof result.then).toBe('function');
    });
});