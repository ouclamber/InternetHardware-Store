import React from 'react';
import { render, screen } from '@testing-library/react';
import CatalogLayout from './CatalogLayout';

describe('CatalogLayout', () => {
    test('рендерит заголовок', () => {
        render(
            <CatalogLayout title="Ноутбуки" filters={<div>Фильтры</div>}>
                <div>Товары</div>
            </CatalogLayout>
        );
        expect(screen.getByText('Ноутбуки')).toBeInTheDocument();
    });

    test('рендерит filters в сайдбаре', () => {
        render(
            <CatalogLayout title="Test" filters={<div>Фильтр цены</div>}>
                <div>Товары</div>
            </CatalogLayout>
        );
        expect(screen.getByText('Фильтр цены')).toBeInTheDocument();
    });

    test('рендерит children', () => {
        render(
            <CatalogLayout title="Test" filters={<div>F</div>}>
                <div>Мои товары</div>
            </CatalogLayout>
        );
        expect(screen.getByText('Мои товары')).toBeInTheDocument();
    });

    test('рендерит pagination', () => {
        render(
            <CatalogLayout
                title="Test"
                filters={<div>F</div>}
                pagination={<div>Страницы</div>}
            >
                <div>Товары</div>
            </CatalogLayout>
        );
        expect(screen.getByText('Страницы')).toBeInTheDocument();
    });
});