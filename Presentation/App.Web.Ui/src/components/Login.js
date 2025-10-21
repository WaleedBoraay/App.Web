import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { authApi } from '../services/apiService';

const Login = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    username: '',
    password: ''
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // Check if user is already logged in and redirect to dashboard
  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (userData) {
      // User is already logged in, redirect to dashboard
      toast.info('أنت مسجل دخول بالفعل. تم توجيهك للوحة التحكم.', {
        position: "top-right",
        autoClose: 3000,
      });
      navigate('/dashboard');
      return;
    }

    // Auto-fill credentials if coming from registration
    const autoFillCredentials = localStorage.getItem('autoFillCredentials');
    if (autoFillCredentials) {
      try {
        const credentials = JSON.parse(autoFillCredentials);
        setFormData({
          username: credentials.email,
          password: credentials.password
        });
        
        // Clear the stored credentials after use
        localStorage.removeItem('autoFillCredentials');
        
        // Show a toast to inform user
        toast.info('Login credentials have been auto-filled from registration.', {
          position: "top-right",
          autoClose: 4000,
        });
      } catch (error) {
        console.error('Error parsing auto-fill credentials:', error);
        localStorage.removeItem('autoFillCredentials');
      }
    }
  }, [navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.username) {
      newErrors.username = 'Username is required';
    }
    
    if (!formData.password) {
      newErrors.password = 'Password is required';
    }
    
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    const newErrors = validateForm();
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    setLoading(true);
    
    try {
      // Call real login API endpoint
      const response = await authApi.login({
        username: formData.username,
        password: formData.password
      });
      
      // Store user data
      localStorage.setItem('user', JSON.stringify({
        username: formData.username,
        ...response // Include any additional data from the API response
      }));
      
      // Store auth token if provided
      if (response.token) {
        localStorage.setItem('authToken', response.token);
      }
      
      // Check isActive status and redirect accordingly
      if (response.isActive === true) {
        // User is active, go to dashboard where they can continue registration
        toast.success('تم تسجيل الدخول بنجاح! يمكنك إكمال التسجيل من لوحة التحكم.', {
          position: "top-right",
          autoClose: 4000,
        });
        
        // Store state to indicate user should start at step 2
        localStorage.setItem('registrationStep', '1'); // Step 2 (0-indexed)
        localStorage.setItem('step1Completed', 'true');
        
        setTimeout(() => navigate('/dashboard'), 1500);
      } else {
        // User registration is complete, go to dashboard
        toast.success('تم تسجيل الدخول بنجاح! مرحباً بك مرة أخرى!', {
          position: "top-right",
          autoClose: 3000,
        });
        
        setTimeout(() => navigate('/dashboard'), 1500);
      }
      
    } catch (error) {
      console.error('Login failed:', error);
      console.error('Error response:', error.response?.data);
      
      // Get error message from API response
      const errorMessage = error.response?.data?.message || error.message;
      
      // Handle different error scenarios with toast notifications
      if (errorMessage === 'User.not.found') {
        toast.error('المستخدم غير موجود. تأكد من البريد الإلكتروني أو قم بالتسجيل أولاً.', {
          position: "top-right",
          autoClose: 6000,
        });
        setErrors({ submit: 'User not found. Please check your email or register first.' });
      } else if (error.response?.status === 401 || errorMessage.includes('password') || errorMessage.includes('credentials')) {
        toast.error('البريد الإلكتروني أو كلمة المرور غير صحيحة.', {
          position: "top-right",
          autoClose: 5000,
        });
        setErrors({ submit: 'Invalid email or password. Please try again.' });
      } else if (error.response?.status === 403) {
        toast.error('الحساب غير مفعل. يرجى التواصل مع الدعم الفني.', {
          position: "top-right",
          autoClose: 5000,
        });
        setErrors({ submit: 'Account is not active. Please contact support.' });
      } else if (error.code === 'ERR_NETWORK') {
        toast.error('مشكلة في الاتصال بالشبكة. تأكد من الاتصال وحاول مرة أخرى.', {
          position: "top-right",
          autoClose: 8000,
        });
        setErrors({ submit: 'Network connection issue. Please try again.' });
      } else {
        toast.error(`خطأ في تسجيل الدخول: ${errorMessage}`, {
          position: "top-right",
          autoClose: 5000,
        });
        setErrors({ submit: `Login failed: ${errorMessage}` });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="gradient-bg"
    style={{background:"#eee"}}>
      <div className="form-container">
        <h2>Login to Your Account</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Enter your username"
            />
            {errors.username && <div className="error">{errors.username}</div>}
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter your password"
            />
            {errors.password && <div className="error">{errors.password}</div>}
          </div>

          {errors.submit && <div className="error">{errors.submit}</div>}

          <button 
            type="submit" 
            className="btn btn-primary btn-full"
            disabled={loading}
          >
            {loading ? <span className="loading"></span> : 'Login'}
          </button>


          <button 
            type="button" 
            className="btn btn-secondary btn-full"
            onClick={() => navigate('/')}
          >
            Back to Welcome
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;
