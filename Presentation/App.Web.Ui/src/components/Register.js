import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import jsPDF from 'jspdf';
import { toast } from 'react-toastify';
import { registrationApi, testApiConnection } from '../services/apiService';

const Register = () => {
  const navigate = useNavigate();
  
  // Check if user is already fully registered and redirect to dashboard
  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        const user = JSON.parse(userData);
        // If user is not active (registration complete), redirect to dashboard
        if (user.isActive === false) {
          toast.info('تم إكمال التسجيل بالفعل. تم توجيهك للوحة التحكم.', {
            position: "top-right",
            autoClose: 3000,
          });
          navigate('/dashboard');
          return;
        }
        // If user is active, they can continue with registration
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
  }, [navigate]);
  
  // Check if user is redirected from login to start at step 2
  const getInitialStep = () => {
    const savedStep = localStorage.getItem('registrationStep');
    return savedStep ? parseInt(savedStep) : 0;
  };
  
  const [currentStep, setCurrentStep] = useState(getInitialStep());
  const [loading, setLoading] = useState(false);
  const [apiLoading, setApiLoading] = useState({
    countries: false,
    licenseSectors: false,
    financialDomains: false
  });
  const [apiData, setApiData] = useState({
    countries: [],
    licenseSectors: [],
    financialDomains: []
  });
  // Check if user is coming from login with step 1 completed
  const getInitialRegistrationState = () => {
    const step1Completed = localStorage.getItem('step1Completed') === 'true';
    const fromDashboard = localStorage.getItem('fromDashboard') === 'true';
    return {
      step1Completed: step1Completed,
      canAccessStep2And3: step1Completed,
      instituteRegistrationResponse: null,
      registrationComplete: false,
      showCredentials: false,
      fromDashboard: fromDashboard
    };
  };
  
  const [registrationState, setRegistrationState] = useState(getInitialRegistrationState());
  
  // Cleanup fromDashboard flag when component unmounts (if registration not complete)
  useEffect(() => {
    return () => {
      // Only clear if registration is not complete
      if (!registrationState.registrationComplete) {
        localStorage.removeItem('fromDashboard');
      }
    };
  }, [registrationState.registrationComplete]);
  
  const [formData, setFormData] = useState({
    // Step 1: Institute Information
    password: '',
    institutionName: '',
    licenseNumber: '',
    licenseSector: '',
    financialType: '',
    licenseIssueDate: '',
    licenseExpiryDate: '',
    country: '',
    address: '',
    email: '',
    businessPhoneNumber: '',
    parentCountry: '',
    parentName: '',
    parentLicenseId: '',
    numberOfEmployees: '',
    
    // Step 2: Contact Person Information
    contactType: '',
    jobTitle: '',
    firstName: '',
    middleName: '',
    lastName: '',
    nationality: '',
    civilId: '',
    passportId: '',
    contactPhone: '',
    businessPhone: '',
    createdBy: 'System Admin',
    registrationDate: new Date().toISOString().split('T')[0],
    applicationStatus: 'Draft',
    submittedTo: '',
    
    // Step 3: Track Actions
    accountType: '',
    initialDeposit: '',
    preferences: {
      emailNotifications: true,
      smsNotifications: false,
      marketingEmails: false
    }
  });
  
  const [uploadedFiles, setUploadedFiles] = useState({
    licenseAttachment: null,
    documentAttachment: null,
    civilIdAttachment: null,
    passportAttachment: null
  });
  const [errors, setErrors] = useState({});

  // Fetch API data on component mount
  useEffect(() => {
    const fetchApiData = async () => {
      console.log('Starting API data fetch...');
      
      // Fetch countries
      setApiLoading(prev => ({ ...prev, countries: true }));
      try {
        console.log('Fetching countries...');
        const countries = await registrationApi.getCountries();
        console.log('Countries response:', countries);
        setApiData(prev => ({ ...prev, countries }));
        console.log('Countries set in state');
      } catch (error) {
        console.error('Failed to fetch countries:', error);
        console.log('Using fallback countries data');
        // Fallback to static data if API fails
        setApiData(prev => ({ ...prev, countries: [
          { id: 1, name: 'Kuwait', twoLetterIsoCode: 'KW' },
          { id: 2, name: 'UAE', twoLetterIsoCode: 'AE' },
          { id: 3, name: 'Saudi Arabia', twoLetterIsoCode: 'SA' },
          { id: 4, name: 'Qatar', twoLetterIsoCode: 'QA' },
          { id: 5, name: 'Bahrain', twoLetterIsoCode: 'BH' },
          { id: 6, name: 'Oman', twoLetterIsoCode: 'OM' }
        ] }));
      } finally {
        setApiLoading(prev => ({ ...prev, countries: false }));
      }

      // Fetch license sectors
      setApiLoading(prev => ({ ...prev, licenseSectors: true }));
      try {
        console.log('Fetching license sectors...');
        const licenseSectors = await registrationApi.getLicenseSectors();
        console.log('License sectors response:', licenseSectors);
        setApiData(prev => ({ ...prev, licenseSectors }));
        console.log('License sectors set in state');
      } catch (error) {
        console.error('Failed to fetch license sectors:', error);
        console.log('Using fallback license sectors data');
        // Fallback to static data if API fails
        setApiData(prev => ({ ...prev, licenseSectors: [
          { id: 1, name: 'Banking' },
          { id: 2, name: 'Exchange' },
          { id: 3, name: 'Insurance' },
          { id: 4, name: 'Investment' }
        ] }));
      } finally {
        setApiLoading(prev => ({ ...prev, licenseSectors: false }));
      }

      // Fetch financial domains
      setApiLoading(prev => ({ ...prev, financialDomains: true }));
      try {
        console.log('Fetching financial domains...');
        const financialDomains = await registrationApi.getFinancialDomains();
        console.log('Financial domains response:', financialDomains);
        setApiData(prev => ({ ...prev, financialDomains }));
        console.log('Financial domains set in state');
      } catch (error) {
        console.error('Failed to fetch financial domains:', error);
        console.log('Using fallback financial domains data');
        // Fallback to static data if API fails
        setApiData(prev => ({ ...prev, financialDomains: [
          { id: 1, name: 'Islamic' },
          { id: 2, name: 'Commercial' },
          { id: 3, name: 'Investment' },
          { id: 4, name: 'Credit Union' }
        ] }));
      } finally {
        setApiLoading(prev => ({ ...prev, financialDomains: false }));
      }
    };

    fetchApiData();
  }, []);

  // Handle redirect from login
  useEffect(() => {
    const isRedirectFromLogin = localStorage.getItem('registrationStep') !== null;
    
    if (isRedirectFromLogin) {
      // Show welcome message for redirected users
      toast.info('Welcome back! Please complete steps 2 and 3 to finish your registration.', {
        position: "top-right",
        autoClose: 5000,
      });
      
      // Clear the redirect flags from localStorage
      localStorage.removeItem('registrationStep');
      // Keep step1Completed flag for this session
    }

    // Cleanup function to clear step1Completed when component unmounts
    return () => {
      // Only clear if user didn't complete the full registration
      if (currentStep < 2) {
        localStorage.removeItem('step1Completed');
      }
    };
  }, [currentStep]);

  const steps = [
    { title: 'Institute Data', key: 'institute' },
    { title: 'Contact Person', key: 'contact' },
    { title: 'Track Actions', key: 'actions' }
  ];

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    
    if (name.startsWith('preferences.')) {
      const prefKey = name.split('.')[1];
      setFormData(prev => ({
        ...prev,
        preferences: {
          ...prev.preferences,
          [prefKey]: checked
        }
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: type === 'checkbox' ? checked : value
      }));
    }
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const handleFileUpload = (fileType) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.pdf,.jpg,.jpeg,.png,.doc,.docx';
    input.onchange = (e) => {
      const file = e.target.files[0];
      if (file) {
        setUploadedFiles(prev => ({
          ...prev,
          [fileType]: file
        }));
      }
    };
    input.click();
  };

  const validateStep = (step) => {
    const newErrors = {};
    
    switch (step) {
      case 0: // Institute Information
        if (!formData.password) {
          newErrors.password = 'Password is required';
        } else if (formData.password.length < 6) {
          newErrors.password = 'Password must be at least 6 characters';
        }
        if (!formData.institutionName) newErrors.institutionName = 'Institution name is required';
        if (!formData.licenseNumber) newErrors.licenseNumber = 'License number is required';
        if (!formData.licenseSector) newErrors.licenseSector = 'License sector is required';
        if (!formData.financialType) newErrors.financialType = 'Financial type is required';
        if (!formData.licenseIssueDate) newErrors.licenseIssueDate = 'License issue date is required';
        if (!formData.licenseExpiryDate) newErrors.licenseExpiryDate = 'License expiry date is required';
        if (!formData.country) newErrors.country = 'Country is required';
        if (!formData.address) newErrors.address = 'Address is required';
        if (!formData.email) {
          newErrors.email = 'Email is required';
        } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
          newErrors.email = 'Email is invalid';
        }
        if (!formData.businessPhoneNumber) newErrors.businessPhoneNumber = 'Business phone number is required';
        break;
        
      case 1: // Contact Person Information
        if (!formData.contactType) newErrors.contactType = 'Contact type is required';
        if (!formData.firstName) newErrors.firstName = 'First name is required';
        if (!formData.lastName) newErrors.lastName = 'Last name is required';
        if (!formData.contactPhone) newErrors.contactPhone = 'Contact phone is required';
        if (!formData.email) {
          newErrors.email = 'Email is required';
        } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
          newErrors.email = 'Email is invalid';
        }
        break;
        
      case 2: // Track actions
        if (!formData.accountType) newErrors.accountType = 'Account type is required';
        if (!formData.initialDeposit) {
          newErrors.initialDeposit = 'Initial deposit is required';
        } else if (isNaN(formData.initialDeposit) || parseFloat(formData.initialDeposit) < 0) {
          newErrors.initialDeposit = 'Initial deposit must be a valid positive number';
        }
        break;
        
      default:
        break;
    }
    
    return newErrors;
  };

  const handleNext = () => {
    const stepErrors = validateStep(currentStep);
    if (Object.keys(stepErrors).length > 0) {
      setErrors(stepErrors);
      return;
    }
    
    // Check if user can access steps 2 and 3
    if (currentStep === 0 && !registrationState.canAccessStep2And3) {
      setErrors({ submit: 'Please complete Step 1 first by clicking the Complete button.' });
      return;
    }
    
    setErrors({});
    setCurrentStep(prev => Math.min(prev + 1, steps.length - 1));
  };

  const handleCompleteStep1 = async () => {
    const stepErrors = validateStep(0);
    if (Object.keys(stepErrors).length > 0) {
      setErrors(stepErrors);
      return;
    }

    setLoading(true);
    
    try {
      // Prepare data for institute registration API as FormData
      const instituteData = {
        name: formData.institutionName,
        email: formData.email,
        password: formData.password,
        businessPhoneNumber: formData.businessPhoneNumber,
        licenseNumber: formData.licenseNumber,
        licenseSector: parseInt(formData.licenseSector),
        financialDomain: parseInt(formData.financialType),
        countryId: parseInt(formData.country),
        address: formData.address,
        issueDate: formData.licenseIssueDate ? new Date(formData.licenseIssueDate).toISOString() : new Date().toISOString(),
        expiryDate: formData.licenseExpiryDate ? new Date(formData.licenseExpiryDate).toISOString() : new Date().toISOString(),
        licenseFile: uploadedFiles.licenseAttachment || null,
        documentFile: uploadedFiles.documentAttachment || null
      };
      
      console.log('Submitting institute registration data:', instituteData);
      console.log('License file object:', uploadedFiles.licenseAttachment);
      console.log('Document file object:', uploadedFiles.documentAttachment);
      
      const response = await registrationApi.instituteRegistration(instituteData);
      
      console.log('Institute registration response:', response);
      
      // Update registration state with the API response
      setRegistrationState(prev => ({
        ...prev,
        step1Completed: true,
        instituteRegistrationResponse: response,
        showCredentials: true // Flag to show credentials UI
      }));
      
      toast.success('Institute registration completed successfully! Your login credentials are ready.', {
        position: "top-right",
        autoClose: 4000,
      });
      
    } catch (error) {
      console.error('Institute registration failed:', error);
      
      // Check if it's a success response but with different structure
      if (error.response?.status === 200 || error.response?.status === 201) {
        setRegistrationState(prev => ({
          ...prev,
          step1Completed: true,
          canAccessStep2And3: true,
          instituteRegistrationResponse: error.response.data
        }));
        
        await fetchInstituteDetails();
        toast.success('Step 1 completed successfully! You can now proceed to steps 2 and 3.', {
          position: "top-right",
          autoClose: 4000,
        });
      } else {
        // Enhanced error handling for network issues
        if (error.message?.includes('Unable to connect to the server') || error.code === 'ERR_NETWORK') {
          toast.error('Network connection issue detected. Please check your internet connection and accept the SSL certificate.', {
            position: "top-right",
            autoClose: 8000,
          });
          
         
        } else {
          toast.error('Institute registration failed. Please try again or contact support.', {
            position: "top-right",
            autoClose: 5000,
          });
        }
        
        setErrors({ submit: 'Institute registration failed. Please try again.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchInstituteDetails = async () => {
    try {
      const params = {
        email: formData.email || "esr@gmail.com",
        NameDescription: formData.institutionName,
        Name: formData.institutionName,
        LicenseNumber: formData.licenseNumber,
        licenseSector: parseInt(formData.licenseSector),
        financialDomain: parseInt(formData.financialType),
        CountryId: parseInt(formData.country),
        Address: formData.address,
        IssueDate: formData.licenseIssueDate ? new Date(formData.licenseIssueDate).toISOString() : new Date().toISOString(),
        ExpiryDate: formData.licenseExpiryDate ? new Date(formData.licenseExpiryDate).toISOString() : new Date().toISOString()
      };
      
      console.log('Fetching institute details with params:', params);
      const instituteDetails = await registrationApi.getInstituteDetails(params);
      console.log('Institute details response:', instituteDetails);
      
      // You can use this data to populate additional fields if needed
      
    } catch (error) {
      console.error('Failed to fetch institute details:', error);
      // Don't block the user if this fails
    }
  };

  const handleBack = () => {
    setCurrentStep(prev => Math.max(prev - 1, 0));
    setErrors({});
  };

  const handleSubmit = async () => {
    const stepErrors = validateStep(currentStep);
    if (Object.keys(stepErrors).length > 0) {
      setErrors(stepErrors);
      return;
    }

    setLoading(true);
    
    try {
      // Use real API endpoint for registration
      const registrationData = {
        ...formData,
        uploadedFiles
      };
      
      const response = await registrationApi.submitRegistration(registrationData);
      
      // Store registration success data
      localStorage.setItem('registrationComplete', JSON.stringify({
        email: formData.email,
        firstName: formData.firstName,
        lastName: formData.lastName,
        id: response.id || Date.now() // fallback ID
      }));
      
      // Clear step1Completed and fromDashboard flags as registration is now complete
      localStorage.removeItem('step1Completed');
      localStorage.removeItem('fromDashboard');
      
      toast.success('Registration completed successfully! You can now login with your credentials.', {
        position: "top-right",
        autoClose: 5000,
      });
      
      // Set registration complete state to show login button
      setRegistrationState(prev => ({
        ...prev,
        registrationComplete: true
      }));
    } catch (error) {
      console.error('Registration failed:', error);
      toast.error('Registration failed. Please try again.', {
        position: "top-right",
        autoClose: 5000,
      });
      setErrors({ submit: 'Registration failed. Please try again.' });
    } finally {
      setLoading(false);
    }
  };

  const handleSaveDraft = async () => {
    setLoading(true);
    
    try {
      // Generate PDF
      const pdf = new jsPDF();
      const pageWidth = pdf.internal.pageSize.getWidth();
      let yPosition = 20;
      
      // Title
      pdf.setFontSize(20);
      pdf.setFont(undefined, 'bold');
      pdf.text('Bank Registration Draft', pageWidth / 2, yPosition, { align: 'center' });
      yPosition += 20;
      
      // Date
      pdf.setFontSize(12);
      pdf.setFont(undefined, 'normal');
      pdf.text(`Generated: ${new Date().toLocaleString()}`, 20, yPosition);
      yPosition += 15;
      
      // Step 1: Institute Information
      pdf.setFontSize(16);
      pdf.setFont(undefined, 'bold');
      pdf.text('Institute Information', 20, yPosition);
      yPosition += 10;
      
      pdf.setFontSize(12);
      pdf.setFont(undefined, 'normal');
      pdf.text(`Institution Name: ${formData.institutionName || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`License Number: ${formData.licenseNumber || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Registration Number: ${formData.registrationNumber || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Tax ID: ${formData.taxId || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Swift Code: ${formData.swiftCode || 'Not provided'}`, 20, yPosition);
      yPosition += 15;
      
      // Step 2: Contact Information
      pdf.setFontSize(16);
      pdf.setFont(undefined, 'bold');
      pdf.text('Contact Information', 20, yPosition);
      yPosition += 10;
      
      pdf.setFontSize(12);
      pdf.setFont(undefined, 'normal');
      pdf.text(`First Name: ${formData.firstName || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Last Name: ${formData.lastName || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Email: ${formData.email || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Phone: ${formData.phone || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Address: ${formData.address || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`City: ${formData.city || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`State: ${formData.state || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Zip Code: ${formData.zipCode || 'Not provided'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Country: ${formData.country || 'Not provided'}`, 20, yPosition);
      yPosition += 15;
      
      // Step 3: Account Setup
      if (yPosition > 250) {
        pdf.addPage();
        yPosition = 20;
      }
      
      pdf.setFontSize(16);
      pdf.setFont(undefined, 'bold');
      pdf.text('Account Setup', 20, yPosition);
      yPosition += 10;
      
      pdf.setFontSize(12);
      pdf.setFont(undefined, 'normal');
      pdf.text(`Account Type: ${formData.accountType || 'Not selected'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Initial Deposit: $${formData.initialDeposit || '0.00'}`, 20, yPosition);
      yPosition += 15;
      
      // Notification Preferences
      pdf.setFontSize(14);
      pdf.setFont(undefined, 'bold');
      pdf.text('Notification Preferences', 20, yPosition);
      yPosition += 10;
      
      pdf.setFontSize(12);
      pdf.setFont(undefined, 'normal');
      pdf.text(`Email Notifications: ${formData.preferences?.emailNotifications ? 'Yes' : 'No'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`SMS Notifications: ${formData.preferences?.smsNotifications ? 'Yes' : 'No'}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Marketing Emails: ${formData.preferences?.marketingEmails ? 'Yes' : 'No'}`, 20, yPosition);
      yPosition += 15;
      
      // Current Progress
      pdf.setFontSize(14);
      pdf.setFont(undefined, 'bold');
      pdf.text('Registration Progress', 20, yPosition);
      yPosition += 10;
      
      pdf.setFontSize(12);
      pdf.setFont(undefined, 'normal');
      pdf.text(`Current Step: ${currentStep + 1} of ${steps.length}`, 20, yPosition);
      yPosition += 8;
      pdf.text(`Progress: ${Math.round(((currentStep + 1) / steps.length) * 100)}%`, 20, yPosition);
      
      // Save PDF
      const fileName = `registration_draft_${new Date().toISOString().split('T')[0]}.pdf`;
      pdf.save(fileName);
      
      // Use real API endpoint for saving draft
      const draftData = {
        ...formData,
        uploadedFiles,
        currentStep,
        timestamp: new Date().toISOString()
      };
      
      await registrationApi.saveDraft(draftData);
      
      toast.success('Draft saved and downloaded as PDF successfully!', {
        position: "top-right",
        autoClose: 4000,
      });
      localStorage.setItem('registrationDraft', JSON.stringify({
        formData,
        uploadedFiles,
        currentStep,
        savedAt: new Date().toISOString()
      }));
    } catch (error) {
      toast.error('Failed to save draft. Please try again.', {
        position: "top-right",
        autoClose: 5000,
      });
    } finally {
      setLoading(false);
    }
  };

  const renderStepContent = () => {
    switch (currentStep) {
      case 0:
        return (
          <div className="step-content">
            <h3>Institute Information</h3>
            <div className="step-grid">
             

              <div className="form-group">
                <label htmlFor="institutionName">Institution Name *</label>
                <input
                  type="text"
                  id="institutionName"
                  name="institutionName"
                  value={formData.institutionName}
                  onChange={handleChange}
                  placeholder="Enter institution name"
                />
                {errors.institutionName && <div className="error">{errors.institutionName}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="licenseNumber">License Number *</label>
                <input
                  type="text"
                  id="licenseNumber"
                  name="licenseNumber"
                  value={formData.licenseNumber}
                  onChange={handleChange}
                  placeholder="Enter license number"
                />
                {errors.licenseNumber && <div className="error">{errors.licenseNumber}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="licenseSector">License Sector</label>
                <select
                  id="licenseSector"
                  name="licenseSector"
                  value={formData.licenseSector}
                  onChange={handleChange}
                  disabled={apiLoading.licenseSectors}
                >
                  <option value="">{apiLoading.licenseSectors ? 'Loading sectors...' : 'Select sector'}</option>
                  {apiData.licenseSectors.map(sector => (
                    <option key={sector.id} value={sector.id}>{sector.name}</option>
                  ))}
                </select>
                {errors.licenseSector && <div className="error">{errors.licenseSector}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="financialType">Financial Type</label>
                <select
                  id="financialType"
                  name="financialType"
                  value={formData.financialType}
                  onChange={handleChange}
                  disabled={apiLoading.financialDomains}
                >
                  <option value="">{apiLoading.financialDomains ? 'Loading types...' : 'Select type'}</option>
                  {apiData.financialDomains.map(domain => (
                    <option key={domain.id} value={domain.id}>{domain.name}</option>
                  ))}
                </select>
                {errors.financialType && <div className="error">{errors.financialType}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="licenseIssueDate">License Issue Date</label>
                <input
                  type="date"
                  id="licenseIssueDate"
                  name="licenseIssueDate"
                  value={formData.licenseIssueDate}
                  onChange={handleChange}
                />
                {errors.licenseIssueDate && <div className="error">{errors.licenseIssueDate}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="licenseExpiryDate">License Expiry Date</label>
                <input
                  type="date"
                  id="licenseExpiryDate"
                  name="licenseExpiryDate"
                  value={formData.licenseExpiryDate}
                  onChange={handleChange}
                />
                {errors.licenseExpiryDate && <div className="error">{errors.licenseExpiryDate}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="country">Country</label>
                <select
                  id="country"
                  name="country"
                  value={formData.country}
                  onChange={handleChange}
                  disabled={apiLoading.countries}
                >
                  <option value="">{apiLoading.countries ? 'Loading countries...' : 'Select country'}</option>
                  {(() => {
                    console.log('Rendering countries dropdown, apiData.countries:', apiData.countries);
                    return apiData.countries.map(country => (
                      <option key={country.id} value={country.id}>{country.name}</option>
                    ));
                  })()}
                </select>
                {errors.country && <div className="error">{errors.country}</div>}
              </div>

           

              <div className="form-group ">
                <label htmlFor="email">Email Address *</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="Enter email address"
                />
                {errors.email && <div className="error">{errors.email}</div>}
              </div>
              <div className="form-group">
                <label htmlFor="password">Password *</label>
                <input
                  type="password"
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="Enter password"
                />
                {errors.password && <div className="error">{errors.password}</div>}
              </div>
              <div className="form-group ">
                <label htmlFor="businessPhoneNumber">Business Phone Number *</label>
                <input
                  type="tel"
                  id="businessPhoneNumber"
                  name="businessPhoneNumber"
                  value={formData.businessPhoneNumber}
                  onChange={handleChange}
                  placeholder="Enter business phone number"
                />
                {errors.businessPhoneNumber && <div className="error">{errors.businessPhoneNumber}</div>}
              </div>
              <div className="form-group full-width">
                <label htmlFor="address">Address *</label>
                <textarea
                  id="address"
                  name="address"
                  value={formData.address}
                  onChange={handleChange}
                  placeholder="Enter complete address"
                  rows="3"
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    border: '1px solid #e1e5e9',
                    borderRadius: '8px',
                    fontSize: '1rem',
                    fontFamily: 'inherit',
                    resize: 'vertical'
                  }}
                />
                {errors.address && <div className="error">{errors.address}</div>}
              </div>
            </div>

            {/* <h4 style={{ marginTop: '2rem', marginBottom: '1rem' }}>Parent Institution Details</h4>
            <div className="form-grid">
              <div className="form-group">
                <label htmlFor="parentCountry">Parent Country</label>
                <select
                  id="parentCountry"
                  name="parentCountry"
                  value={formData.parentCountry}
                  onChange={handleChange}
                  disabled={apiLoading.countries}
                >
                  <option value="">{apiLoading.countries ? 'Loading countries...' : 'Select parent country'}</option>
                  {apiData.countries.map(country => (
                    <option key={country.id} value={country.id}>{country.name}</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="parentName">Parent Name</label>
                <input
                  type="text"
                  id="parentName"
                  name="parentName"
                  value={formData.parentName}
                  onChange={handleChange}
                  placeholder="Enter parent institution name"
                />
              </div>

              <div className="form-group">
                <label htmlFor="parentLicenseId">Parent License ID</label>
                <input
                  type="text"
                  id="parentLicenseId"
                  name="parentLicenseId"
                  value={formData.parentLicenseId}
                  onChange={handleChange}
                  placeholder="Enter parent license ID"
                />
              </div>

              <div className="form-group">
                <label htmlFor="numberOfEmployees">Number of Employees</label>
                <input
                  type="number"
                  id="numberOfEmployees"
                  name="numberOfEmployees"
                  value={formData.numberOfEmployees}
                  onChange={handleChange}
                  placeholder="Enter number"
                />
              </div>
            </div> */}

            <h4 style={{ marginTop: '2rem', marginBottom: '1rem' }}>Document Attachments</h4>
            <div className="step-grid-2">
              <div style={{ textAlign: 'center', padding: '2rem', border: '2px dashed #e1e5e9', borderRadius: '8px' }}>
                <div style={{ fontSize: '2rem', marginBottom: '1rem', color: '#999' }}>📄</div>
                <div style={{ fontWeight: 'bold', marginBottom: '0.5rem' }}>License Attachment</div>
                <div style={{ fontSize: '0.875rem', color: '#666', marginBottom: '1rem' }}>Upload license document</div>
                {uploadedFiles.licenseAttachment ? (
                  <div style={{ marginBottom: '1rem' }}>
                    <div style={{ fontSize: '0.875rem', color: '#28a745', fontWeight: 'bold' }}>
                      ✓ {uploadedFiles.licenseAttachment.name}
                    </div>
                    <div style={{ fontSize: '0.75rem', color: '#666' }}>
                      {(uploadedFiles.licenseAttachment.size / 1024 / 1024).toFixed(2)} MB
                    </div>
                  </div>
                ) : null}
                <button 
                  type="button" 
                  className="btn btn-primary" 
                  style={{ padding: '0.5rem 1rem', fontSize: '0.875rem' }}
                  onClick={() => handleFileUpload('licenseAttachment')}
                >
                  {uploadedFiles.licenseAttachment ? 'Change File' : 'Upload File'}
                </button>
              </div>

              <div style={{ textAlign: 'center', padding: '2rem', border: '2px dashed #e1e5e9', borderRadius: '8px' }}>
                <div style={{ fontSize: '2rem', marginBottom: '1rem', color: '#999' }}>📎</div>
                <div style={{ fontWeight: 'bold', marginBottom: '0.5rem' }}>Document Attachment</div>
                <div style={{ fontSize: '0.875rem', color: '#666', marginBottom: '1rem' }}>Upload supporting document</div>
                {uploadedFiles.documentAttachment ? (
                  <div style={{ marginBottom: '1rem' }}>
                    <div style={{ fontSize: '0.875rem', color: '#28a745', fontWeight: 'bold' }}>
                      ✓ {uploadedFiles.documentAttachment.name}
                    </div>
                    <div style={{ fontSize: '0.75rem', color: '#666' }}>
                      {(uploadedFiles.documentAttachment.size / 1024 / 1024).toFixed(2)} MB
                    </div>
                  </div>
                ) : null}
                <button 
                  type="button" 
                  className="btn btn-primary" 
                  style={{ padding: '0.5rem 1rem', fontSize: '0.875rem' }}
                  onClick={() => handleFileUpload('documentAttachment')}
                >
                  {uploadedFiles.documentAttachment ? 'Change File' : 'Upload File'}
                </button>
              </div>
            </div>

            <h4 style={{ marginTop: '2rem', marginBottom: '1rem' }}>Notification Preferences</h4>
            <div className="form-group">
              <div style={{ 
                background: '#f8fafc', 
                padding: '1.5rem', 
                borderRadius: '12px', 
                border: '1px solid #e2e8f0' 
              }}>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: '1rem' }}>
                  <label style={{ 
                    display: 'flex', 
                    alignItems: 'center', 
                    padding: '0.75rem',
                    background: 'white',
                    borderRadius: '8px',
                    border: '1px solid #e5e7eb',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease',
                    boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)'
                  }}
                  onMouseEnter={(e) => {
                    e.target.style.borderColor = '#667eea';
                    e.target.style.boxShadow = '0 2px 4px rgba(102, 126, 234, 0.1)';
                  }}
                  onMouseLeave={(e) => {
                    e.target.style.borderColor = '#e5e7eb';
                    e.target.style.boxShadow = '0 1px 2px rgba(0, 0, 0, 0.05)';
                  }}>
                    <input
                      type="checkbox"
                      name="preferences.emailNotifications"
                      checked={formData.preferences.emailNotifications}
                      onChange={handleChange}
                      style={{ 
                        marginRight: '0.75rem',
                        width: '18px',
                        height: '18px',
                        accentColor: '#667eea',
                        cursor: 'pointer'
                      }}
                    />
                    <div>
                      <div style={{ fontWeight: '500', color: '#374151' }}>📧 Email Notifications</div>
                      <div style={{ fontSize: '0.875rem', color: '#6b7280' }}>Receive updates and alerts via email</div>
                    </div>
                  </label>
                  
                  <label style={{ 
                    display: 'flex', 
                    alignItems: 'center', 
                    padding: '0.75rem',
                    background: 'white',
                    borderRadius: '8px',
                    border: '1px solid #e5e7eb',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease',
                    boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)'
                  }}
                  onMouseEnter={(e) => {
                    e.target.style.borderColor = '#667eea';
                    e.target.style.boxShadow = '0 2px 4px rgba(102, 126, 234, 0.1)';
                  }}
                  onMouseLeave={(e) => {
                    e.target.style.borderColor = '#e5e7eb';
                    e.target.style.boxShadow = '0 1px 2px rgba(0, 0, 0, 0.05)';
                  }}>
                    <input
                      type="checkbox"
                      name="preferences.smsNotifications"
                      checked={formData.preferences.smsNotifications}
                      onChange={handleChange}
                      style={{ 
                        marginRight: '0.75rem',
                        width: '18px',
                        height: '18px',
                        accentColor: '#667eea',
                        cursor: 'pointer'
                      }}
                    />
                    <div>
                      <div style={{ fontWeight: '500', color: '#374151' }}>📱 SMS Notifications</div>
                      <div style={{ fontSize: '0.875rem', color: '#6b7280' }}>Receive important alerts via text message</div>
                    </div>
                  </label>
                  
                  <label style={{ 
                    display: 'flex', 
                    alignItems: 'center', 
                    padding: '0.75rem',
                    background: 'white',
                    borderRadius: '8px',
                    border: '1px solid #e5e7eb',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease',
                    boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)'
                  }}
                  onMouseEnter={(e) => {
                    e.target.style.borderColor = '#667eea';
                    e.target.style.boxShadow = '0 2px 4px rgba(102, 126, 234, 0.1)';
                  }}
                  onMouseLeave={(e) => {
                    e.target.style.borderColor = '#e5e7eb';
                    e.target.style.boxShadow = '0 1px 2px rgba(0, 0, 0, 0.05)';
                  }}>
                    <input
                      type="checkbox"
                      name="preferences.marketingEmails"
                      checked={formData.preferences.marketingEmails}
                      onChange={handleChange}
                      style={{ 
                        marginRight: '0.75rem',
                        width: '18px',
                        height: '18px',
                        accentColor: '#667eea',
                        cursor: 'pointer'
                      }}
                    />
                    <div>
                      <div style={{ fontWeight: '500', color: '#374151' }}>📬 Marketing Emails</div>
                      <div style={{ fontSize: '0.875rem', color: '#6b7280' }}>Receive promotional offers and updates</div>
                    </div>
                  </label>
                </div>
              </div>
            </div>

            {/* Complete Step 1 Button */}
            <div style={{ 
              marginTop: '2rem', 
              padding: '1.5rem', 
              background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)', 
              borderRadius: '12px',
              textAlign: 'center'
            }}>
              <h4 style={{ color: 'white', marginBottom: '1rem' }}>Complete Institute Registration</h4>
              <p style={{ color: 'rgba(255,255,255,0.9)', marginBottom: '1.5rem', fontSize: '0.9rem' }}>
                Click the button below to submit your institute information and proceed with the registration process.
              </p>
              <button
                type="button"
                className="btn"
                onClick={handleCompleteStep1}
                disabled={loading}
                style={{
                  background: 'white',
                  color: '#667eea',
                  border: 'none',
                  padding: '0.75rem 2rem',
                  borderRadius: '8px',
                  fontWeight: 'bold',
                  fontSize: '1rem',
                  cursor: loading ? 'not-allowed' : 'pointer',
                  opacity: loading ? 0.7 : 1,
                  transition: 'all 0.2s ease'
                }}
                onMouseEnter={(e) => {
                  if (!loading) {
                    e.target.style.transform = 'translateY(-2px)';
                    e.target.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
                  }
                }}
                onMouseLeave={(e) => {
                  if (!loading) {
                    e.target.style.transform = 'translateY(0)';
                    e.target.style.boxShadow = 'none';
                  }
                }}
              >
                {loading ? (
                  <>
                    <span className="loading" style={{ marginRight: '0.5rem' }}></span>
                    Processing...
                  </>
                ) : (
                  <>
                    {registrationState.step1Completed ? '✓ Completed' : 'Complete Step 1'}
                  </>
                )}
              </button>
              
              {registrationState.step1Completed && registrationState.showCredentials && registrationState.instituteRegistrationResponse && (
                <div style={{ 
                  marginTop: '1rem', 
                  padding: '1rem', 
                  background: 'rgba(255,255,255,0.15)', 
                  borderRadius: '8px',
                  color: 'white',
                  fontSize: '0.875rem'
                }}>
                  <div style={{ textAlign: 'center', marginBottom: '1rem' }}>
                    <div style={{ fontSize: '1.2rem', marginBottom: '0.5rem' }}>✓</div>
                    <strong>Registration Completed Successfully!</strong>
                  </div>
                  
                  <div style={{ 
                    background: 'rgba(255,255,255,0.1)', 
                    padding: '1rem', 
                    borderRadius: '6px', 
                    marginBottom: '1rem',
                    border: '1px solid rgba(255,255,255,0.2)'
                  }}>
                    <div style={{ marginBottom: '0.5rem', display: 'flex', justifyContent: 'space-between' }}>
                      <strong>Email:</strong> 
                      <span style={{ fontFamily: 'monospace' }}>{registrationState.instituteRegistrationResponse.email}</span>
                    </div>
                    <div style={{ marginBottom: '0.5rem', display: 'flex', justifyContent: 'space-between' }}>
                      <strong>Password:</strong> 
                      <span style={{ fontFamily: 'monospace' }}>{registrationState.instituteRegistrationResponse.password}</span>
                    </div>
                    <div style={{ fontSize: '0.75rem', color: 'rgba(255,255,255,0.8)', marginTop: '0.5rem', textAlign: 'center' }}>
                      Save these credentials for login
                    </div>
                  </div>
                  
                  <button
                    type="button"
                    onClick={() => {
                      // Store credentials for auto-fill
                      localStorage.setItem('autoFillCredentials', JSON.stringify({
                        email: registrationState.instituteRegistrationResponse.email,
                        password: registrationState.instituteRegistrationResponse.password
                      }));
                      navigate('/login');
                    }}
                    style={{
                      width: '100%',
                      background: 'white',
                      color: '#667eea',
                      border: 'none',
                      padding: '0.75rem',
                      borderRadius: '6px',
                      fontWeight: 'bold',
                      cursor: 'pointer',
                      transition: 'all 0.2s ease'
                    }}
                    onMouseEnter={(e) => {
                      e.target.style.transform = 'translateY(-1px)';
                      e.target.style.boxShadow = '0 2px 8px rgba(255,255,255,0.3)';
                    }}
                    onMouseLeave={(e) => {
                      e.target.style.transform = 'translateY(0)';
                      e.target.style.boxShadow = 'none';
                    }}
                  >
                    Go to Login
                  </button>
                </div>
              )}
              
              {registrationState.step1Completed && !registrationState.showCredentials && (
                <div style={{ 
                  marginTop: '1rem', 
                  padding: '0.75rem', 
                  background: 'rgba(255,255,255,0.2)', 
                  borderRadius: '8px',
                  color: 'white',
                  fontSize: '0.875rem'
                }}>
                  ✓ Institute registration completed successfully!
                  {registrationState.canAccessStep2And3 && (
                    <div style={{ marginTop: '0.5rem' }}>
                      You can now proceed to steps 2 and 3.
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        );

      case 1:
        return (
          <div className="step-content">
            <h3>Contact Person Information</h3>
            <div className="form-grid">
              <div className="form-group">
                <label htmlFor="contactType">Contact Type *</label>
                <select
                  id="contactType"
                  name="contactType"
                  value={formData.contactType}
                  onChange={handleChange}
                >
                  <option value="">Select contact type</option>
                  <option value="primary">Primary Contact</option>
                  <option value="secondary">Secondary Contact</option>
                  <option value="authorized">Authorized Representative</option>
                  <option value="compliance">Compliance Officer</option>
                </select>
                {errors.contactType && <div className="error">{errors.contactType}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="jobTitle">Job Title *</label>
                <input
                  type="text"
                  id="jobTitle"
                  name="jobTitle"
                  value={formData.jobTitle}
                  onChange={handleChange}
                  placeholder="Enter job title"
                />
                {errors.jobTitle && <div className="error">{errors.jobTitle}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="firstName">First Name *</label>
                <input
                  type="text"
                  id="firstName"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  placeholder="First name"
                />
                {errors.firstName && <div className="error">{errors.firstName}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="middleName">Middle Name</label>
                <input
                  type="text"
                  id="middleName"
                  name="middleName"
                  value={formData.middleName}
                  onChange={handleChange}
                  placeholder="Middle name"
                />
              </div>

              <div className="form-group">
                <label htmlFor="lastName">Last Name *</label>
                <input
                  type="text"
                  id="lastName"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  placeholder="Last name"
                />
                {errors.lastName && <div className="error">{errors.lastName}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="nationality">Nationality</label>
                <select
                  id="nationality"
                  name="nationality"
                  value={formData.nationality}
                  onChange={handleChange}
                  disabled={apiLoading.countries}
                >
                  <option value="">{apiLoading.countries ? 'Loading nationalities...' : 'Select nationality'}</option>
                  {apiData.countries.map(country => (
                    <option key={country.id} value={country.id}>{country.name}</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="civilId">Civil ID</label>
                <input
                  type="text"
                  id="civilId"
                  name="civilId"
                  value={formData.civilId}
                  onChange={handleChange}
                  placeholder="Enter civil ID"
                />
              </div>

              <div className="form-group">
                <label htmlFor="passportId">Passport ID</label>
                <input
                  type="text"
                  id="passportId"
                  name="passportId"
                  value={formData.passportId}
                  onChange={handleChange}
                  placeholder="Enter passport ID"
                />
              </div>

              <div className="form-group">
                <label htmlFor="contactPhone">Contact Phone *</label>
                <input
                  type="tel"
                  id="contactPhone"
                  name="contactPhone"
                  value={formData.contactPhone}
                  onChange={handleChange}
                  placeholder="Enter contact phone"
                />
                {errors.contactPhone && <div className="error">{errors.contactPhone}</div>}
              </div>

              <div className="form-group">
                <label htmlFor="businessPhone">Business Phone</label>
                <input
                  type="tel"
                  id="businessPhone"
                  name="businessPhone"
                  value={formData.businessPhone}
                  onChange={handleChange}
                  placeholder="Enter business phone"
                />
              </div>

              <div className="form-group ">
                <label htmlFor="email">Email Address *</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="Enter email address"
                />
                {errors.email && <div className="error">{errors.email}</div>}
              </div>
            </div>

            <h4 style={{ marginTop: '2rem', marginBottom: '1rem' }}>Document Attachments</h4>
            <div className="form-grid-card">
              <div style={{ textAlign: 'center', padding: '2rem', border: '2px dashed #e1e5e9', borderRadius: '8px' }}>
                <div style={{ fontSize: '2rem', marginBottom: '1rem', color: '#999' }}>🆔</div>
                <div style={{ fontWeight: 'bold', marginBottom: '0.5rem' }}>Civil ID Attachment</div>
                <div style={{ fontSize: '0.875rem', color: '#666', marginBottom: '1rem' }}>Upload civil ID document</div>
                {uploadedFiles.civilIdAttachment ? (
                  <div style={{ marginBottom: '1rem' }}>
                    <div style={{ fontSize: '0.875rem', color: '#28a745', fontWeight: 'bold' }}>
                      ✓ {uploadedFiles.civilIdAttachment.name}
                    </div>
                    <div style={{ fontSize: '0.75rem', color: '#666' }}>
                      {(uploadedFiles.civilIdAttachment.size / 1024 / 1024).toFixed(2)} MB
                    </div>
                  </div>
                ) : null}
                <button 
                  type="button" 
                  className="btn btn-primary" 
                  style={{ padding: '0.5rem 1rem', fontSize: '0.875rem' }}
                  onClick={() => handleFileUpload('civilIdAttachment')}
                >
                  {uploadedFiles.civilIdAttachment ? 'Change File' : 'Upload File'}
                </button>
              </div>

              <div style={{ textAlign: 'center', padding: '2rem', border: '2px dashed #e1e5e9', borderRadius: '8px' }}>
                <div style={{ fontSize: '2rem', marginBottom: '1rem', color: '#999' }}>📘</div>
                <div style={{ fontWeight: 'bold', marginBottom: '0.5rem' }}>Passport Attachment</div>
                <div style={{ fontSize: '0.875rem', color: '#666', marginBottom: '1rem' }}>Upload passport document</div>
                {uploadedFiles.passportAttachment ? (
                  <div style={{ marginBottom: '1rem' }}>
                    <div style={{ fontSize: '0.875rem', color: '#28a745', fontWeight: 'bold' }}>
                      ✓ {uploadedFiles.passportAttachment.name}
                    </div>
                    <div style={{ fontSize: '0.75rem', color: '#666' }}>
                      {(uploadedFiles.passportAttachment.size / 1024 / 1024).toFixed(2)} MB
                    </div>
                  </div>
                ) : null}
                <button 
                  type="button" 
                  className="btn btn-primary" 
                  style={{ padding: '0.5rem 1rem', fontSize: '0.875rem' }}
                  onClick={() => handleFileUpload('passportAttachment')}
                >
                  {uploadedFiles.passportAttachment ? 'Change File' : 'Upload File'}
                </button>
              </div>
            </div>

          
          </div>
        );

      case 2:
        return (
          <div className="step-content">
            <h3>Track Actions</h3>
            <div className="track-actions-grid">
              {/* Validation Section */}
              <div className="track-card validation-card">
                <div className="track-card-header">
                  <div className="track-icon validation-icon">
                    <span>!</span>
                  </div>
                  <div>
                    <div className="track-title">Validation</div>
                    <div className="track-status validation-status">Pending</div>
                  </div>
                </div>
                <div className="track-description validation-description">
                  Document validation is in progress. Please ensure all required documents are uploaded.
                </div>
              </div>

              {/* Approval Section */}
              <div className="track-card waiting-card">
                <div className="track-card-header">
                  <div className="track-icon waiting-icon">
                    <span>⏳</span>
                  </div>
                  <div>
                    <div className="track-title">Approval</div>
                    <div className="track-status waiting-status">Waiting</div>
                  </div>
                </div>
                <div className="track-description waiting-description">
                  Approval process will begin after validation is complete.
                </div>
              </div>

              {/* Audit Section */}
              <div className="track-card waiting-card">
                <div className="track-card-header">
                  <div className="track-icon waiting-icon">
                    <span>📋</span>
                  </div>
                  <div>
                    <div className="track-title">Audit</div>
                    <div className="track-status waiting-status">Waiting</div>
                  </div>
                </div>
                <div className="track-description waiting-description">
                  Audit process will begin after approval is granted.
                </div>
              </div>
            </div>

            <div className="registration-summary-card">
              <div className="summary-header">
                <div className="summary-left">
                  <div className="registration-id">
                    <i className="fas fa-id-card"></i>
                    Registration ID: REG-2024-001
                  </div>
                  <div className="registration-date">
                    <i className="fas fa-calendar"></i>
                    Date: {new Date().toLocaleDateString()}
                  </div>
                </div>
                <div className="summary-right">
                  <div className="status-badge draft-status">
                    <i className="fas fa-edit"></i>
                    Draft
                  </div>
                  <div className="progress-container">
                    <div className="progress-label">Progress: 66%</div>
                    <div className="progress-bar">
                      <div className="progress-fill" style={{ width: '66%' }}></div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="summary-message">
                <i className="fas fa-info-circle"></i>
                Your application is being processed. You will receive notifications at each stage of the review process.
              </div>
            </div>

            <div className="form-group" style={{ marginTop: '2rem' }}>
              <label htmlFor="accountType">Account Type</label>
              <select
                id="accountType"
                name="accountType"
                value={formData.accountType}
                onChange={handleChange}
              >
                <option value="">Select account type</option>
                <option value="savings">Savings Account</option>
                <option value="checking">Checking Account</option>
                <option value="business">Business Account</option>
                <option value="student">Student Account</option>
              </select>
              {errors.accountType && <div className="error">{errors.accountType}</div>}
            </div>

            <div className="form-group">
              <label htmlFor="initialDeposit">Initial Deposit ($)</label>
              <input
                type="number"
                id="initialDeposit"
                name="initialDeposit"
                value={formData.initialDeposit}
                onChange={handleChange}
                placeholder="Enter initial deposit amount"
                min="0"
                step="0.01"
              />
              {errors.initialDeposit && <div className="error">{errors.initialDeposit}</div>}
            </div>

          </div>
        );

      default:
        return null;
    }
  };

  // Show success screen if registration is complete
  if (registrationState.registrationComplete) {
    return (
      <div className="gradient-bg"
      style={{background:"#fff"}}>
        <div className="form-container" style={{ textAlign: 'center', maxWidth: '500px' }}>
          <div style={{ marginBottom: '2rem' }}>
            <div style={{
              width: '80px',
              height: '80px',
              borderRadius: '50%',
              background: 'linear-gradient(135deg, #28a745 0%, #20c997 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              margin: '0 auto 1rem',
              fontSize: '2rem',
              color: 'white'
            }}>
              ✓
            </div>
            <h2 style={{ color: '#28a745', marginBottom: '1rem' }}>Registration Successful!</h2>
            <p style={{ color: '#666', marginBottom: '2rem' }}>
              Your account has been created successfully. You can now login with your registered email.
            </p>
            <div style={{ 
              background: '#f8f9fa', 
              padding: '1rem', 
              borderRadius: '8px', 
              marginBottom: '2rem',
              border: '1px solid #e9ecef'
            }}>
              <p><strong>Registered Email:</strong> {formData.email}</p>
            </div>
          </div>
          
          <button
            type="button"
            className="btn btn-primary btn-full"
            onClick={() => navigate('/login')}
            style={{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              marginBottom: '1rem'
            }}
          >
            Go to Login
          </button>
          
          <button
            type="button"
            className="btn btn-secondary btn-full"
            onClick={() => navigate('/')}
          >
            Back to Welcome
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="gradient-bg"
    style={{background:"#eee"}}>
    
      <div className="stepper-container" >
        <h2 style={{margin: '20px 5px'}}>Registeration Progress</h2>
        
      
        {/* Custom Stepper */}
        <div style={{ marginBottom: '2rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', marginBottom: '1rem' }}>
            {steps.map((step, index) => (
              <React.Fragment key={step.key}>
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', flex: 1 }}>
                  <div
                    style={{
                      width: '40px',
                      height: '40px',
                      borderRadius: '50%',
                      background: index === 0 && registrationState.step1Completed
                        ? 'linear-gradient(135deg, #28a745 0%, #20c997 100%)'
                        : index <= currentStep 
                          ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' 
                          : '#e1e5e9',
                      color: index <= currentStep || (index === 0 && registrationState.step1Completed) ? 'white' : '#999',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontWeight: 'bold',
                      fontSize: '1.2rem',
                      transition: 'all 0.3s ease',
                      border: index === 0 && registrationState.step1Completed ? '2px solid #28a745' : 'none'
                    }}
                  >
                    {index === 0 && registrationState.step1Completed ? '✓' : index < currentStep ? '✓' : index + 1}
                  </div>
                  <div
                    style={{
                      fontSize: '0.75rem',
                      color: index <= currentStep ? '#667eea' : '#999',
                      fontWeight: index === currentStep ? 'bold' : 'normal',
                      textAlign: 'center',
                      marginTop: '0.5rem',
                      maxWidth: '100px'
                    }}
                  >
                    {step.title}
                  </div>
                </div>
               
                {index < steps.length - 1 && (
                  <div
                    style={{
                      flex: 1,
                      height: '2px',
                      background: index < currentStep 
                        ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' 
                        : '#e1e5e9',
                      margin: '0 1rem',
                      marginTop: '-20px',
                      transition: 'all 0.3s ease'
                    }}
                  />
                )}
              </React.Fragment>
            ))}
          </div>
        </div>
        
        {renderStepContent()}

        {errors.submit && <div className="error">{errors.submit}</div>}

        <div className="step-navigation">
          {!registrationState.fromDashboard ? (
            <button
              type="button"
              className="btn btn-secondary"
              onClick={currentStep === 0 ? () => navigate('/') : handleBack}
            >
              {currentStep === 0 ? 'Back to Welcome' : 'Back'}
            </button>
          ) : (
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => {
                localStorage.removeItem('fromDashboard');
                navigate('/dashboard');
              }}
            >
              <i className="fas fa-arrow-left" style={{ marginRight: '0.5rem' }}></i>
              Back to Dashboard
            </button>
          )}

          <div className="button-group">
            {currentStep === steps.length - 1 && (
              <button
                type="button"
                className="btn btn-secondary save-draft-btn"
                onClick={handleSaveDraft}
                disabled={loading}
              >
                {loading ? <span className="loading"></span> : (
                  <>
                    <span>💾</span>
                    Save Draft
                  </>
                )}
              </button>
            )}
            
            {currentStep < steps.length - 1 ? (
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleNext}
                disabled={currentStep === 0 && !registrationState.canAccessStep2And3}
                style={{
                  opacity: currentStep === 0 && !registrationState.canAccessStep2And3 ? 0.5 : 1,
                  cursor: currentStep === 0 && !registrationState.canAccessStep2And3 ? 'not-allowed' : 'pointer'
                }}
              >
                {currentStep === 0 && !registrationState.canAccessStep2And3 ? 'Complete Step 1 First' : 'Next'}
              </button>
            ) : (
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleSubmit}
                disabled={loading}
              >
                {loading ? <span className="loading"></span> : 'Complete Registration'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
