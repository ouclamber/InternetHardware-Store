import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ProductGallery from './ProductGallery';

jest.mock('shared/api/baseUrl', () => ({
    API_URL: 'http://localhost:5214'
}));

const mockProduct = { Name: 'MacBook Air' };

describe('ProductGallery', () => {
    test('рендерит главную картинку absolute URL', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="https://example.com/img.jpg"
                images={[]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(screen.getByAltText('MacBook Air')).toHaveAttribute(
            'src',
            'https://example.com/img.jpg'
        );
    });

    test('рендерит главную картинку relative URL + API_URL', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="http://localhost:5214/uploads/img.jpg"
                images={[]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(screen.getByAltText('MacBook Air')).toHaveAttribute(
            'src',
            'http://localhost:5214/uploads/img.jpg'
        );
    });

    test('показывает placeholder при отсутствии mainImage', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage={null}
                images={[]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(screen.getByText('Нет изображения')).toBeInTheDocument();
    });

    test('не рендерит миниатюры при 1 картинке', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/main.jpg' }]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelector('.image-thumbnails')).not.toBeInTheDocument();
    });

    test('рендерит миниатюры при 2+ картинках', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelectorAll('.thumbnail').length).toBe(2);
    });

    test('миниатюры с imageUrl (camelCase)', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ imageUrl: '/img1.jpg' }, { imageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelectorAll('.thumbnail').length).toBe(2);
    });

    test('активная миниатюра — если selectedImage совпадает', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage="http://localhost:5214/img1.jpg"
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelectorAll('.thumbnail.active').length).toBe(1);
    });

    test('нет активной если selectedImage = null', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelectorAll('.thumbnail.active').length).toBe(0);
    });

    test('клик на миниатюру вызывает onImageSelect с absolute URL', () => {
        const onImageSelect = jest.fn();
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={onImageSelect}
            />
        );
        fireEvent.click(document.querySelectorAll('.thumbnail')[0]);
        expect(onImageSelect).toHaveBeenCalledWith('http://localhost:5214/img1.jpg');
    });

    test('клик на вторую миниатюру', () => {
        const onImageSelect = jest.fn();
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={onImageSelect}
            />
        );
        fireEvent.click(document.querySelectorAll('.thumbnail')[1]);
        expect(onImageSelect).toHaveBeenCalledWith('http://localhost:5214/img2.jpg');
    });

    test('клик на миниатюру без слеша', () => {
        const onImageSelect = jest.fn();
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: 'img1.jpg' }, { ImageUrl: 'img2.jpg' }]}
                selectedImage={null}
                onImageSelect={onImageSelect}
            />
        );
        fireEvent.click(document.querySelectorAll('.thumbnail')[0]);
        expect(onImageSelect).toHaveBeenCalledWith('http://localhost:5214/img1.jpg');
    });

    test('alt на миниатюрах содержит имя товара и индекс', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(screen.getByAltText('MacBook Air 1')).toBeInTheDocument();
        expect(screen.getByAltText('MacBook Air 2')).toBeInTheDocument();
    });

    test('onError главной картинки заменяет src на placeholder', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="http://localhost:5214/broken.jpg"
                images={[]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        const img = screen.getByAltText('MacBook Air');
        fireEvent.error(img);
        expect(img.src).toContain('via.placeholder');
    });

    test('onError миниатюры скрывает её', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[{ ImageUrl: '/img1.jpg' }, { ImageUrl: '/img2.jpg' }]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        const thumbnails = document.querySelectorAll('.thumbnail img');
        fireEvent.error(thumbnails[0]);
        expect(thumbnails[0].style.display).toBe('none');
    });

    test('рендерит контейнер product-gallery', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelector('.product-gallery')).toBeInTheDocument();
    });

    test('рендерит main-image контейнер', () => {
        render(
            <ProductGallery
                product={mockProduct}
                mainImage="/main.jpg"
                images={[]}
                selectedImage={null}
                onImageSelect={jest.fn()}
            />
        );
        expect(document.querySelector('.main-image')).toBeInTheDocument();
    });
});