import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import QuantityControl from './QuantityControl';

describe('QuantityControl', () => {
    const mockProps = {
        quantity: 5,
        onIncrease: jest.fn(),
        onDecrease: jest.fn(),
        onChange: jest.fn(),
        disabled: false,
        min: 1,
        showInput: true
    };

    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит текущее количество', () => {
        render(<QuantityControl {...mockProps} />);
        expect(screen.getByDisplayValue('5')).toBeInTheDocument();
    });

    test('вызывает onIncrease при клике на "+"', () => {
        render(<QuantityControl {...mockProps} />);
        const buttons = screen.getAllByRole('button');
        const plusButton = buttons[1]; // Второй — плюс
        fireEvent.click(plusButton);
        expect(mockProps.onIncrease).toHaveBeenCalled();
    });

    test('вызывает onDecrease при клике на "-"', () => {
        render(<QuantityControl {...mockProps} />);
        const buttons = screen.getAllByRole('button');
        const minusButton = buttons[0]; // Первый — минус
        fireEvent.click(minusButton);
        expect(mockProps.onDecrease).toHaveBeenCalled();
    });

    test('кнопка "-" отключена если quantity <= min', () => {
        render(<QuantityControl {...mockProps} quantity={1} min={1} />);
        const buttons = screen.getAllByRole('button');
        expect(buttons[0]).toBeDisabled();
    });

    test('кнопка "+" отключена если disabled', () => {
        render(<QuantityControl {...mockProps} disabled={true} />);
        const buttons = screen.getAllByRole('button');
        expect(buttons[1]).toBeDisabled();
    });

    test('input отключён если disabled', () => {
        render(<QuantityControl {...mockProps} disabled={true} />);
        expect(screen.getByRole('spinbutton')).toBeDisabled();
    });

    test('вызывает onChange при изменении input', () => {
        render(<QuantityControl {...mockProps} />);
        const input = screen.getByRole('spinbutton');
        fireEvent.change(input, { target: { value: '10' } });
        expect(mockProps.onChange).toHaveBeenCalled();
    });

    test('при showInput=false рендерит span вместо input', () => {
        render(<QuantityControl {...mockProps} showInput={false} />);
        expect(screen.queryByRole('spinbutton')).not.toBeInTheDocument();
        expect(screen.getByText('5')).toBeInTheDocument();
    });

    test('кнопки без текста при showInput=false (только SVG)', () => {
        render(<QuantityControl {...mockProps} showInput={false} />);
        const buttons = screen.getAllByRole('button');
        expect(buttons.length).toBe(2);
    });
});