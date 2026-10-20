import { useProductHelpers } from './useProductHelpers';

const { getProductImage, getBrandName } = useProductHelpers();

describe('useProductHelpers', () => {
    describe('getProductImage', () => {
        test('возвращает ImageUrl (PascalCase) с приоритетом', () => {
            const product = { ImageUrl: '/img.jpg', imageUrl: '/other.jpg' };
            expect(getProductImage(product)).toBe('http://localhost:5214/img.jpg');
        });

        test('возвращает imageUrl, если ImageUrl нет', () => {
            const product = { imageUrl: '/img.jpg' };
            expect(getProductImage(product)).toBe('http://localhost:5214/img.jpg');
        });

        test('ImageUrl приоритетнее imageUrl', () => {
            const product = { imageUrl: '/priority.jpg', ImageUrl: '/other.jpg' };
            expect(getProductImage(product)).toBe('http://localhost:5214/other.jpg');
        });

        test('возвращает полный URL, если он absolute', () => {
            const product = { ImageUrl: 'https://priority.com/img.jpg' };
            expect(getProductImage(product)).toBe('https://priority.com/img.jpg');
        });

        test('fallback на Images[] если ImageUrl нет', () => {
            const product = {
                Images: [
                    { ImageUrl: '/first.jpg', IsMain: false },
                    { ImageUrl: '/main.jpg', IsMain: true }
                ]
            };
            expect(getProductImage(product)).toBe('http://localhost:5214/main.jpg');
        });

        test('берёт первый Images[], если IsMain не найден', () => {
            const product = {
                Images: [
                    { ImageUrl: '/first.jpg' },
                    { ImageUrl: '/second.jpg' }
                ]
            };
            expect(getProductImage(product)).toBe('http://localhost:5214/first.jpg');
        });

        test('возвращает null для пустого продукта', () => {
            expect(getProductImage({})).toBeNull();
        });

        test('возвращает null для "string" (баг backend)', () => {
            const product = { ImageUrl: 'string' };
            expect(getProductImage(product)).toBeNull();
        });

        test('добавляет слеш, если URL относительный без слеша', () => {
            const product = { ImageUrl: 'img.jpg' };
            expect(getProductImage(product)).toBe('http://localhost:5214/img.jpg');
        });
    });

    describe('getBrandName', () => {
        test('возвращает BrandName из продукта', () => {
            const product = { BrandName: 'ASUS' };
            expect(getBrandName(product)).toBe('ASUS');
        });

        test('берёт Brand.Name если BrandName нет', () => {
            const product = { Brand: { Name: 'Apple' } };
            expect(getBrandName(product)).toBe('Apple');
        });

        test('определяет бренд по имени товара через defaultBrands', () => {
            const product = { Name: 'MacBook Air M3' };
            const defaultBrands = { 'macbook': 'Apple', 'asus': 'ASUS' };
            expect(getBrandName(product, defaultBrands)).toBe('Apple');
        });

        test('возвращает "Не указан", если бренд не определён', () => {
            const product = { Name: 'Неизвестный товар' };
            expect(getBrandName(product)).toBe('Не указан');
        });
    });
});