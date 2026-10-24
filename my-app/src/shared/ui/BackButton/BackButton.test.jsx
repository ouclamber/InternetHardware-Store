import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import BackButton from './BackButton';

describe('BackButton', () => {
    test('рендерит текст "Назад" по умолчанию', () => {
        render(<BackButton onClick={() => {}} />);
        expect(screen.getByText('Назад')).toBeInTheDocument();
    });

    test('рендерит кастомный текст', () => {
        render(<BackButton onClick={() => {}} text="На главную" />);
        expect(screen.getByText('На главную')).toBeInTheDocument();
    });

    test('вызывает onClick при клике', () => {
        const onClick = jest.fn();
        render(<BackButton onClick={onClick} />);
        fireEvent.click(screen.getByRole('button'));
        expect(onClick).toHaveBeenCalledTimes(1);
    });

    test('добавляет кастомный className', () => {
        render(<BackButton onClick={() => {}} className="custom-class" />);
        const button = screen.getByRole('button');
        expect(button).toHaveClass('back-button');
        expect(button).toHaveClass('custom-class');
    });

    test('без className добавляет только "back-button"', () => {
        render(<BackButton onClick={() => {}} />);
        expect(screen.getByRole('button')).toHaveClass('back-button');
    });
});