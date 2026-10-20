import React from 'react';
import { render, screen } from '@testing-library/react';
import ProductReviews from './ProductReviews';

jest.mock('./ReviewForm', () => () => <div data-testid="review-form" />);
jest.mock('./ReviewItem', () => ({ review }) => (
    <div data-testid={`review-${review.id}`}>{review.comment}</div>
));

const defaultProps = {
    reviews: [],
    reviewsLoading: false,
    averageRating: 0,
    totalReviews: 0,
    newReview: { rating: 5, comment: '' },
    reviewSubmitting: false,
    editingReview: null,
    editComment: '',
    editRating: 5,
    editSubmitting: false,
    currentUserId: '1',
    userRole: 'User',
    onRatingChange: jest.fn(),
    onCommentChange: jest.fn(),
    onSubmitReview: jest.fn(),
    onStartEdit: jest.fn(),
    onCancelEdit: jest.fn(),
    onSaveEdit: jest.fn(),
    onChangeEditComment: jest.fn(),
    onChangeEditRating: jest.fn(),
    onDeleteReview: jest.fn()
};

describe('ProductReviews', () => {
    test('рендерит заголовок "Отзывы"', () => {
        render(<ProductReviews {...defaultProps} />);
        expect(screen.getByText('Отзывы')).toBeInTheDocument();
    });

    test('не показывает averageRating при 0 отзывов', () => {
        render(<ProductReviews {...defaultProps} />);
        expect(screen.queryByText(/\(\d+ отзывов\)/)).not.toBeInTheDocument();
    });

    test('показывает averageRating при наличии отзывов', () => {
        render(<ProductReviews {...defaultProps} averageRating={4.5} totalReviews={10} />);
        expect(screen.getByText('4.5 (10 отзывов)')).toBeInTheDocument();
    });

    test('рендерит ReviewForm', () => {
        render(<ProductReviews {...defaultProps} />);
        expect(screen.getByTestId('review-form')).toBeInTheDocument();
    });

    test('показывает "Загрузка отзывов..." при reviewsLoading', () => {
        render(<ProductReviews {...defaultProps} reviewsLoading={true} />);
        expect(screen.getByText('Загрузка отзывов...')).toBeInTheDocument();
    });

    test('показывает "Отзывов пока нет" при пустом списке', () => {
        render(<ProductReviews {...defaultProps} reviews={[]} />);
        expect(screen.getByText('Отзывов пока нет. Будьте первым!')).toBeInTheDocument();
    });

    test('рендерит список отзывов', () => {
        render(<ProductReviews {...defaultProps} reviews={[
            { id: 1, userId: 1, userName: 'Иван', rating: 5, comment: 'Отличный!' },
            { id: 2, userId: 2, userName: 'Пётр', rating: 4, comment: 'Хороший' }
        ]} />);
        expect(screen.getByTestId('review-1')).toHaveTextContent('Отличный!');
        expect(screen.getByTestId('review-2')).toHaveTextContent('Хороший');
    });

    test('не показывает loading при наличии отзывов', () => {
        render(<ProductReviews {...defaultProps} reviews={[
            { id: 1, userId: 1, userName: 'Иван', rating: 5, comment: 'Хороший' }
        ]} />);
        expect(screen.queryByText('Загрузка отзывов...')).not.toBeInTheDocument();
    });
});