# Email Verification Setup Guide

## Overview
The Predictive Maintenance system now includes robust account creation with email verification and validation. This ensures only valid and unique emails can register and log in.

## Features Implemented

### 1. User Registration with Email Verification
- **Email Format Validation**: Only valid email formats are accepted
- **Unique Email Enforcement**: Prevents duplicate email registrations
- **Unique Username**: Prevents duplicate usernames
- **Unique Worker ID**: Ensures each worker ID is unique across the system
- **Real-time Validation**: AJAX-powered client-side validation for immediate feedback

### 2. Email Verification System
- **Verification Token**: Secure token-based email verification
- **Token Expiration**: 24-hour expiration for security
- **Resend Functionality**: Users can request new verification emails
- **Login Blocking**: Unverified users cannot log in

### 3. Enhanced User Experience
- **Modern UI**: Responsive and visually appealing registration form
- **Real-time Feedback**: Instant validation for email, username, and worker ID
- **Password Strength Meter**: Visual feedback for password strength
- **Terms and Conditions**: Mandatory acceptance checkbox

## Configuration

### Email Settings (appsettings.json)
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Predictive Maintenance System",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

### For Gmail Setup:
1. Enable 2-Factor Authentication on your Gmail account
2. Generate an App Password:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate a password for "Mail"
3. Use the generated app password in the configuration

### For Development (Mock Email):
For testing without actual email sending, you can:
1. Use a service like MailTrap.io
2. Configure a local SMTP server
3. Implement a file-based email service for development

## How It Works

### Registration Flow:
1. User fills out the registration form
2. Real-time validation checks:
   - Email format and uniqueness
   - Username uniqueness
   - Worker ID uniqueness
   - Password strength
3. Upon form submission:
   - Account is created (unverified)
   - Verification email is sent
   - User is redirected to login with instructions

### Email Verification Flow:
1. User receives email with verification link
2. Clicking the link verifies the account
3. User can now log in successfully

### Login Flow:
1. User attempts to log in
2. System checks if email is verified
3. If unverified: Shows error with resend option
4. If verified: Proceeds with normal login

## API Endpoints

### Real-time Validation Endpoints:
- `GET /User/CheckEmailExists?email={email}` - Check if email exists
- `GET /User/CheckUsernameExists?username={username}` - Check if username exists
- `GET /User/CheckWorkerIdExists?workerId={workerId}` - Check if worker ID exists

### Email Verification Endpoints:
- `GET /User/VerifyEmail?userId={userId}&token={token}` - Verify email
- `GET /User/ResendVerification?email={email}` - Resend verification email

## Testing the System

### Manual Testing Steps:
1. **Start the application**: `dotnet run`
2. **Navigate to registration**: `http://localhost:5261/User/Create`
3. **Test real-time validation**:
   - Enter an invalid email format
   - Try existing usernames/emails
   - Test password strength meter
4. **Complete registration**:
   - Fill valid information
   - Submit form
   - Check for success message
5. **Test email verification**:
   - Check email logs (if configured)
   - Test verification URL manually
6. **Test login flow**:
   - Try logging in before verification
   - Verify the resend email option works
   - Complete verification and test successful login

### Database Schema Changes:
The following fields were added to the `AspNetUsers` table:
- `IsEmailVerified` (bit, default: false)
- `EmailVerificationToken` (nvarchar(max), nullable)
- `EmailVerificationTokenExpires` (datetime2, nullable)

### Security Features:
- **Token-based verification**: Secure random token generation
- **Time-limited tokens**: 24-hour expiration prevents replay attacks
- **Password policies**: Enforced strong password requirements
- **Unique constraints**: Prevents duplicate accounts
- **Input validation**: Both client-side and server-side validation

## Troubleshooting

### Common Issues:
1. **Email not sending**: Check SMTP configuration and credentials
2. **Verification link not working**: Ensure tokens match and haven't expired
3. **Real-time validation not working**: Check JavaScript console for errors
4. **Database errors**: Ensure migration has been applied

### Error Messages:
- "Please verify your email address before logging in" - User needs to verify email
- "An account with this email address already exists" - Email already registered
- "This username is already taken" - Username already exists
- "This Worker ID is already registered" - Worker ID already exists

## Future Enhancements

Potential improvements:
1. **Password reset functionality** with email verification
2. **Email change verification** for profile updates
3. **Account lockout** after failed verification attempts
4. **Admin panel** for managing user verification status
5. **Bulk user import** with automatic email verification
6. **Email templates** with branding and better formatting
