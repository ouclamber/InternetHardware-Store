import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ReviewForm from './ReviewForm';

const defaultProps = {
    newReview: { rating: 5, comment: '' },
    reviewSubmitting: false,
    onRatingChange: jest.fn(),
    onCommentChange: jest.fn(),
    onSubmitReview: jest.fn()
};

describe('ReviewForm', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит textarea для комментария', () => {
        render(<ReviewForm {...defaultProps} />);
        const textarea = screen.getByRole('textbox');
        expect(textarea).toBeInTheDocument();
    });

    test('изменение комментария вызывает onCommentChange', () => {
        render(<ReviewForm {...defaultProps} />);
        const textarea = screen.getByRole('textbox');
        fireEvent.change(textarea, { target: { value: 'Отличный товар!' } });
        expect(defaultProps.onCommentChange).toHaveBeenCalledWith('Отличный товар!');
    });

    test('рендерит кнопки', () => {
        render(<ReviewForm {...defaultProps} />);
        const buttons = screen.getAllByRole('button');
        expect(buttons.length).toBeGreaterThan(0);
    });

    test('отображает текущий комментарий', () => {
        render(
            <ReviewForm
                {...defaultProps}
                newReview={{ rating: 5, comment: 'Уже введённый текст' }}
            />
        );
        const textarea = screen.getByRole('textbox');
        expect(textarea).toHaveValue('Уже введённый текст');
    });

    test('не падает при reviewSubmitting=true', () => {
        render(<ReviewForm {...defaultProps} reviewSubmitting={true} />);
        expect(screen.getByRole('textbox')).toBeInTheDocument();
    });
});