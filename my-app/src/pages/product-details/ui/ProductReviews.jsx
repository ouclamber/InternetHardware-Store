import React from 'react';
import './ProductReviews.css';
import ReviewForm from './ReviewForm';
import ReviewItem from './ReviewItem';

const ProductReviews = ({
    reviews,
    reviewsLoading,
    averageRating,
    totalReviews,
    newReview,
    reviewSubmitting,
    editingReview,
    editComment,
    editRating,
    editSubmitting,
    currentUserId,
    userRole,
    onRatingChange,
    onCommentChange,
    onSubmitReview,
    onStartEdit,
    onCancelEdit,
    onSaveEdit,
    onChangeEditComment,
    onChangeEditRating,
    onDeleteReview
}) => {
    return (
        <div className="product-reviews">
            <h2 className="reviews-title">
                Отзывы
                {totalReviews > 0 && (
                    <span className="average-rating">
                        {averageRating} ({totalReviews} отзывов)
                    </span>
                )}
            </h2>

            <ReviewForm
                newReview={newReview}
                reviewSubmitting={reviewSubmitting}
                onRatingChange={onRatingChange}
                onCommentChange={onCommentChange}
                onSubmit={onSubmitReview}
            />

            {reviewsLoading ? (
                <div className="reviews-loading">Загрузка отзывов...</div>
            ) : reviews.length > 0 ? (
                <div className="reviews-list">
                    {reviews.map(review => {
                        const canEditDelete =
                            currentUserId === review.userId?.toString() || userRole === 'Admin';

                        return (
                            <ReviewItem
                                key={review.id}
                                review={review}
                                isEditing={editingReview && editingReview.id === review.id}
                                editComment={editComment}
                                editRating={editRating}
                                editSubmitting={editSubmitting}
                                canEditDelete={canEditDelete}
                                onStartEdit={() => onStartEdit(review)}
                                onCancelEdit={onCancelEdit}
                                onSaveEdit={onSaveEdit}
                                onChangeComment={onChangeEditComment}
                                onChangeRating={onChangeEditRating}
                                onDelete={() => onDeleteReview(review.id)}
                            />
                        );
                    })}
                </div>
            ) : (
                <p className="no-reviews">Отзывов пока нет. Будьте первым!</p>
            )}
        </div>
    );
};

export default ProductReviews;