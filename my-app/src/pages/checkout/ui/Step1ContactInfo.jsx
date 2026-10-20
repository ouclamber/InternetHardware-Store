import React from 'react';

const Step1ContactInfo = ({ formData, fieldErrors, onInputChange, onPhoneBlur }) => {
    return (
        <div className="checkout-step">
            <h2 className="step-title">Контактная информация</h2>

            <div className="form-row">
                <div className="form-group">
                    <label>Имя</label>
                    <input
                        type="text"
                        name="firstName"
                        value={formData.firstName}
                        onChange={onInputChange}
                        className={`form-input ${fieldErrors.firstName ? 'error' : ''}`}
                        placeholder="Введите имя"
                    />
                    {fieldErrors.firstName && (
                        <span className="error-message">{fieldErrors.firstName}</span>
                    )}
                </div>

                <div className="form-group">
                    <label>Фамилия</label>
                    <input
                        type="text"
                        name="lastName"
                        value={formData.lastName}
                        onChange={onInputChange}
                        className={`form-input ${fieldErrors.lastName ? 'error' : ''}`}
                        placeholder="Введите фамилию"
                    />
                    {fieldErrors.lastName && (
                        <span className="error-message">{fieldErrors.lastName}</span>
                    )}
                </div>
            </div>

            <div className="form-row">
                <div className="form-group">
                    <label>Email</label>
                    <input
                        type="email"
                        name="email"
                        value={formData.email}
                        onChange={onInputChange}
                        className={`form-input ${fieldErrors.email ? 'error' : ''}`}
                        placeholder="example@mail.com"
                    />
                    {fieldErrors.email && (
                        <span className="error-message">{fieldErrors.email}</span>
                    )}
                </div>

                <div className="form-group">
                    <label>Телефон</label>
                    <input
                        type="tel"
                        name="phone"
                        value={formData.phone}
                        onChange={onInputChange}
                        onBlur={onPhoneBlur}
                        className={`form-input ${fieldErrors.phone ? 'error' : ''}`}
                        placeholder="+7 (999) 123-45-67"
                    />
                    {fieldErrors.phone && (
                        <span className="error-message">{fieldErrors.phone}</span>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Step1ContactInfo;