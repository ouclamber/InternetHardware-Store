import { getImageUrl } from './getImageUrl';

jest.mock('shared/api/baseUrl', () => ({
    API_URL: 'http://localhost:5214'
}));

describe('getImageUrl', () => {
    test('возвращает null для null/undefined', () => {
        expect(getImageUrl(null)).toBeNull();
        expect(getImageUrl(undefined)).toBeNull();
        expect(getImageUrl('')).toBeNull();
    });

    test('возвращает absolute URL без изменений', () => {
        expect(getImageUrl('https://example.com/img.jpg'))
            .toBe('https://example.com/img.jpg');
    });

    test('добавляет API_URL к относительному URL', () => {
        expect(getImageUrl('/images/test.jpg'))
            .toBe('http://localhost:5214/images/test.jpg');
    });

    test('добавляет слеш, если его нет', () => {
        expect(getImageUrl('images/test.jpg'))
            .toBe('http://localhost:5214/images/test.jpg');
    });

    test('возвращает null для "string"', () => {
        expect(getImageUrl('string')).toBeNull();
    });
});