import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ErrorMessage from './ErrorMessage';

describe('ErrorMessage', () => {
    test('рендерит текст ошибки', () => {
        render(<ErrorMessage message="Что-то пошло не так" />);
        expect(screen.getByText('Что-то пошло не так')).toBeInTheDocument();
    });

    test('рендерит кнопку "Попробовать снова" если onRetry передан', () => {
        render(<ErrorMessage message="Ошибка" onRetry={() => {}} />);
        expect(screen.getByText('Попробовать снова')).toBeInTheDocument();
    });

    test('не рендерит кнопку "Попробовать снова" без onRetry', () => {
        render(<ErrorMessage message="Ошибка" />);
        expect(screen.queryByText('Попробовать снова')).not.toBeInTheDocument();
    });

    test('рендерит кнопку "Очистить кэш" если onClearCache передан', () => {
        render(<ErrorMessage message="Ошибка" onClearCache={() => {}} />);
        expect(screen.getByText('Очистить кэш')).toBeInTheDocument();
    });

    test('вызывает onRetry при клике на кнопку', () => {
        const onRetry = jest.fn();
        render(<ErrorMessage message="Ошибка" onRetry={onRetry} />);
        fireEvent.click(screen.getByText('Попробовать снова'));
        expect(onRetry).toHaveBeenCalledTimes(1);
    });

    test('вызывает onClearCache при клике на кнопку', () => {
        const onClearCache = jest.fn();
        render(<ErrorMessage message="Ошибка" onClearCache={onClearCache} />);
        fireEvent.click(screen.getByText('Очистить кэш'));
        expect(onClearCache).toHaveBeenCalledTimes(1);
    });
});