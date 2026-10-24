import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import CategoryCard from './CategoryCard';

const mockCategory = {
    id: 1,
    name: 'Ноутбуки',
    description: 'Лучшие ноутбуки',
    productCount: 10,
    imageUrl: '/images/laptop.jpg'
};

const defaultProps = {
    category: mockCategory,
    index: 0,
    onClick: jest.fn(),
    getCategoryImage: () => null
};

describe('CategoryCard', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит название категории', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(screen.getByText('Ноутбуки')).toBeInTheDocument();
    });

    test('рендерит описание', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(screen.getByText('Лучшие ноутбуки')).toBeInTheDocument();
    });

    test('рендерит "Смотреть товары"', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(screen.getByText('Смотреть товары')).toBeInTheDocument();
    });

    test('productCount = 10 → "10 товаров"', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(screen.getByText('10 товаров')).toBeInTheDocument();
    });

    test('productCount = 1 → "1 товар"', () => {
        render(<CategoryCard {...defaultProps} category={{ ...mockCategory, productCount: 1 }} />);
        expect(screen.getByText('1 товар')).toBeInTheDocument();
    });

    test('productCount = 0 → "0 товаров"', () => {
        render(<CategoryCard {...defaultProps} category={{ ...mockCategory, productCount: 0 }} />);
        expect(screen.getByText('0 товаров')).toBeInTheDocument();
    });

    test('productCount = 5 → "5 товаров"', () => {
        render(<CategoryCard {...defaultProps} category={{ ...mockCategory, productCount: 5 }} />);
        expect(screen.getByText('5 товаров')).toBeInTheDocument();
    });

    test('класс empty при productCount = 0', () => {
        render(<CategoryCard {...defaultProps} category={{ ...mockCategory, productCount: 0 }} />);
        expect(document.querySelector('.category-card').classList.contains('empty')).toBe(true);
    });

    test('нет класса empty при productCount > 0', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(document.querySelector('.category-card').classList.contains('empty')).toBe(false);
    });

    test('клик вызывает onClick с id и name', () => {
        const onClick = jest.fn();
        render(<CategoryCard {...defaultProps} onClick={onClick} />);
        fireEvent.click(document.querySelector('.category-card'));
        expect(onClick).toHaveBeenCalledWith(1, 'Ноутбуки');
    });

    test('рендерит картинку при getCategoryImage', () => {
        render(
            <CategoryCard
                {...defaultProps}
                getCategoryImage={() => 'http://localhost:5214/img.jpg'}
            />
        );
        const img = document.querySelector('.category-image');
        expect(img).toHaveAttribute('src', 'http://localhost:5214/img.jpg');
    });

    test('рендерит placeholder при null image', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(document.querySelector('.category-no-image')).toBeInTheDocument();
    });

    test('alt картинки = название категории', () => {
        render(
            <CategoryCard
                {...defaultProps}
                getCategoryImage={() => 'http://localhost:5214/img.jpg'}
            />
        );
        expect(screen.getByAltText('Ноутбуки')).toBeInTheDocument();
    });

    test('не рендерит описание если его нет', () => {
        const { description, ...noDesc } = mockCategory;
        render(<CategoryCard {...defaultProps} category={noDesc} />);
        expect(screen.queryByText(description)).not.toBeInTheDocument();
    });

    test('обрезает описание до 80 символов', () => {
        const longDesc = 'a'.repeat(100);
        render(
            <CategoryCard {...defaultProps} category={{ ...mockCategory, description: longDesc }} />
        );
        const el = document.querySelector('.category-description');
        expect(el.textContent.length).toBe(83);
    });

    test('короткое описание не обрезается', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(screen.getByText('Лучшие ноутбуки')).toBeInTheDocument();
    });

    test('index 0 → category-color-1', () => {
        const { container } = render(<CategoryCard {...defaultProps} index={0} />);
        expect(container.querySelector('.category-card').className).toMatch(/category-color-1/);
    });

    test('index 4 → category-color-5', () => {
        const { container } = render(<CategoryCard {...defaultProps} index={4} />);
        expect(container.querySelector('.category-card').className).toMatch(/category-color-5/);
    });

    test('index 5 → category-color-1 (циклично)', () => {
        const { container } = render(<CategoryCard {...defaultProps} index={5} />);
        expect(container.querySelector('.category-card').className).toMatch(/category-color-1/);
    });

    test('onError картинки скрывает её', () => {
        render(
            <CategoryCard
                {...defaultProps}
                getCategoryImage={() => 'http://localhost:5214/broken.jpg'}
            />
        );
        const img = document.querySelector('.category-image');
        expect(img).toBeInTheDocument();

        fireEvent.error(img);

        expect(img.style.display).toBe('none');
    });

    test('getCategoryImage вызывается с category', () => {
        const getCategoryImage = jest.fn(() => null);
        render(<CategoryCard {...defaultProps} getCategoryImage={getCategoryImage} />);
        expect(getCategoryImage).toHaveBeenCalledWith(mockCategory);
    });

    test('рендерит стрелку в "Смотреть товары"', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(document.querySelector('.arrow-icon')).toBeInTheDocument();
    });

    test('рендерит category-overlay', () => {
        render(<CategoryCard {...defaultProps} />);
        expect(document.querySelector('.category-overlay')).toBeInTheDocument();
    });
});