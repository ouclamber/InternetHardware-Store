import React from 'react';
import { render, screen } from '@testing-library/react';
import LoadingSpinner from './LoadingSpinner';

describe('LoadingSpinner', () => {
    test('рендерит текст по умолчанию', () => {
        render(<LoadingSpinner />);
        expect(screen.getByText('Загрузка...')).toBeInTheDocument();
    });

    test('рендерит кастомный текст', () => {
        render(<LoadingSpinner text="Загрузка товаров..." />);
        expect(screen.getByText('Загрузка товаров...')).toBeInTheDocument();
    });

    test('рендерит спиннер с классом "spinner"', () => {
        const { container } = render(<LoadingSpinner />);
        expect(container.querySelector('.spinner')).toBeInTheDocument();
    });
});