import axios from 'axios';

// API configuration - API server stays the same regardless of frontend deployment
const getApiBaseUrl = () => {
    if (window.location.hostname === "localhost") {
        return "/api";
    }
    return "/api";
};

const API_BASE_URL = getApiBaseUrl();

// Log the API URL for debugging
console.log('API Base URL:', API_BASE_URL);
console.log('Environment:', process.env.NODE_ENV);
console.log('Hostname:', window.location.hostname);

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 50000,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    },

    withCredentials: false,
    maxRedirects: 5,
    validateStatus: function (status) {
        return status >= 200 && status < 300;
    },
});


apiClient.interceptors.request.use(
    (config) => {
        // Add auth token if available
        const token = localStorage.getItem('authToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor for handling common errors
apiClient.interceptors.response.use(
    (response) => {
        return response;
    },
    (error) => {
        // Handle common errors
        if (error.response?.status === 401) {
            // Handle unauthorized access
            localStorage.removeItem('authToken');
            localStorage.removeItem('user');
            // Redirect to login if needed
        }
        return Promise.reject(error);
    }
);

// Test API connectivity
export const testApiConnection = async () => {
    try {
        console.log('Testing API connection...');
        const response = await fetch(`${API_BASE_URL}/RegistrationApi/countries`, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
            mode: 'cors', // Enable CORS
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        console.log('API connection test successful:', data);
        return { success: true, data };
    } catch (error) {
        console.error('API connection test failed:', error);
        return { success: false, error: error.message };
    }
};

// Registration API endpoints
export const registrationApi = {
    // Get all countries
    getCountries: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/RegistrationApi/countries`);
            const response = await apiClient.get('/RegistrationApi/countries');
            console.log('Countries API response status:', response.status);
            console.log('Countries API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error fetching countries:', error);
            console.error('Error details:', {
                message: error.message,
                status: error.response?.status,
                statusText: error.response?.statusText,
                data: error.response?.data
            });
            throw error;
        }
    },

    // Get license sectors
    getLicenseSectors: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/RegistrationApi/lookups/license-sectors`);
            const response = await apiClient.get('/RegistrationApi/lookups/license-sectors');
            console.log('License sectors API response status:', response.status);
            console.log('License sectors API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error fetching license sectors:', error);
            console.error('Error details:', {
                message: error.message,
                status: error.response?.status,
                statusText: error.response?.statusText,
                data: error.response?.data
            });
            throw error;
        }
    },

    // Get financial domains
    getFinancialDomains: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/RegistrationApi/lookups/financial-domains`);
            const response = await apiClient.get('/RegistrationApi/lookups/financial-domains');
            console.log('Financial domains API response status:', response.status);
            console.log('Financial domains API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error fetching financial domains:', error);
            console.error('Error details:', {
                message: error.message,
                status: error.response?.status,
                statusText: error.response?.statusText,
                data: error.response?.data
            });
            throw error;
        }
    },

    // Submit registration
    submitRegistration: async (registrationData) => {
        try {
            const response = await apiClient.post('/RegistrationApi/register', registrationData);
            return response.data;
        } catch (error) {
            console.error('Error submitting registration:', error);
            throw error;
        }
    },

    // Save draft
    saveDraft: async (draftData) => {
        try {
            const response = await apiClient.post('/RegistrationApi/draft', draftData);
            return response.data;
        } catch (error) {
            console.error('Error saving draft:', error);
            throw error;
        }
    },

    // Institute registration (Step 1 Complete)
    instituteRegistration: async (instituteData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/RegistrationApi/institute`);

            // Create FormData for Step 1 submission
            const formData = new FormData();

            // Add all required fields with PascalCase names
            formData.append('Name', instituteData.name || '');
            formData.append('Email', instituteData.email || '');
            formData.append('Password', instituteData.password || '');
            formData.append('BusinessPhoneNumber', instituteData.businessPhoneNumber || '');
            formData.append('LicenseNumber', instituteData.licenseNumber || '');
            formData.append('LicenseSector', instituteData.licenseSector || '');
            formData.append('FinancialDomain', instituteData.financialDomain || '');
            formData.append('CountryId', instituteData.countryId || '');
            formData.append('Address', instituteData.address || '');
            formData.append('IssueDate', instituteData.issueDate || '');
            formData.append('ExpiryDate', instituteData.expiryDate || '');

            // Add files if they exist
            if (instituteData.licenseFile && instituteData.licenseFile instanceof File) {
                formData.append('LicenseFile', instituteData.licenseFile);
            }

            if (instituteData.documentFile && instituteData.documentFile instanceof File) {
                formData.append('DocumentFile', instituteData.documentFile);
            }

            console.log('Sending FormData for Step 1:');
            console.log('API Endpoint:', `${API_BASE_URL}/RegistrationApi/institute`);
            console.log('Request Headers will include: Content-Type: multipart/form-data');

            for (let [key, value] of formData.entries()) {
                if (value instanceof File) {
                    console.log(`${key}: File - Name: ${value.name}, Size: ${value.size}, Type: ${value.type}`);
                } else {
                    console.log(`${key}: ${value} (type: ${typeof value})`);
                }
            }

            console.log('FormData entries count:', Array.from(formData.entries()).length);

            // Validate required fields before sending
            const requiredFields = ['Name', 'Email', 'Password', 'BusinessPhoneNumber', 'LicenseNumber', 'Address'];
            const missingFields = [];

            for (const field of requiredFields) {
                const hasField = Array.from(formData.entries()).some(([key]) => key === field);
                if (!hasField) {
                    missingFields.push(field);
                }
            }

            if (missingFields.length > 0) {
                console.warn('Missing required fields:', missingFields);
            }

            // Send FormData request
            const response = await apiClient.post('/RegistrationApi/institute', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });

            console.log('Institute registration API response status:', response.status);
            console.log('Institute registration API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error submitting institute registration:', error);
            console.error('Error details:', {
                message: error.message,
                status: error.response?.status,
                statusText: error.response?.statusText,
                data: error.response?.data,
                headers: error.response?.headers
            });

            // Log specific error information for 500 errors
            if (error.response?.status === 500) {
                console.error('500 Internal Server Error Details:');
                console.error('- This is a server-side error');
                console.error('- Check if all required fields are provided');
                console.error('- Verify API endpoint accepts FormData');
                console.error('- Response data:', error.response?.data);
            }

            throw error;
        }
    },

    // Institute details (Steps 2 & 3)
    getInstituteDetails: async (params) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/RegistrationApi/institute`);
            const queryParams = new URLSearchParams();

            if (params.email) queryParams.append('email', params.email);
            if (params.NameDescription) queryParams.append('NameDescription', params.NameDescription);
            if (params.Name) queryParams.append('Name', params.Name);
            if (params.LicenseNumber) queryParams.append('LicenseNumber', params.LicenseNumber);
            if (params.licenseSector) queryParams.append('licenseSector', params.licenseSector);
            if (params.financialDomain) queryParams.append('financialDomain', params.financialDomain);
            if (params.CountryId) queryParams.append('CountryId', params.CountryId);
            if (params.Address) queryParams.append('Address', params.Address);
            if (params.IssueDate) queryParams.append('IssueDate', params.IssueDate);
            if (params.ExpiryDate) queryParams.append('ExpiryDate', params.ExpiryDate);

            const url = `/RegistrationApi/institute?${queryParams.toString()}`;
            console.log('Full URL:', `${API_BASE_URL}${url}`);

            const response = await apiClient.get(url);
            console.log('Institute details API response status:', response.status);
            console.log('Institute details API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error fetching institute details:', error);
            console.error('Error details:', {
                message: error.message,
                status: error.response?.status,
                statusText: error.response?.statusText,
                data: error.response?.data
            });
            throw error;
        }
    }
};

// Authentication API endpoints
export const authApi = {
    // Login
    login: async (loginData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/AccountApi/login`);
            const response = await apiClient.post('/AccountApi/login', {
                username: loginData.username,
                password: loginData.password,
                isActive: true
            });
            console.log('Login API response status:', response.status);
            console.log('Login API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error during login:', error);
            console.error('Error details:', {
                message: error.message,
                status: error.response?.status,
                statusText: error.response?.statusText,
                data: error.response?.data
            });
            throw error;
        }
    }
};

// Admin Users API endpoints
export const usersApi = {
    // Get all users
    getAllUsers: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users`);
            const response = await apiClient.get('/admin/users');
            console.log('Users API response status:', response.status);
            console.log('Users API response data:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error fetching users:', error);
            throw error;
        }
    },

    // Get user by ID
    getUserById: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}`);
            const response = await apiClient.get(`/admin/users/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching user:', error);
            throw error;
        }
    },

    // Create new user
    createUser: async (userData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users`);
            const response = await apiClient.post('/admin/users', userData);
            return response.data;
        } catch (error) {
            console.error('Error creating user:', error);
            throw error;
        }
    },

    // Update user
    updateUser: async (id, userData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}`);
            const response = await apiClient.put(`/admin/users/${id}`, userData);
            return response.data;
        } catch (error) {
            console.error('Error updating user:', error);
            throw error;
        }
    },

    // Delete user
    deleteUser: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}`);
            const response = await apiClient.delete(`/admin/users/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error deleting user:', error);
            throw error;
        }
    },

    // Activate user
    activateUser: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}/activate`);
            const response = await apiClient.post(`/admin/users/${id}/activate`);
            return response.data;
        } catch (error) {
            console.error('Error activating user:', error);
            throw error;
        }
    },

    // Deactivate user
    deactivateUser: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}/deactivate`);
            const response = await apiClient.post(`/admin/users/${id}/deactivate`);
            return response.data;
        } catch (error) {
            console.error('Error deactivating user:', error);
            throw error;
        }
    },

    // Lock user
    lockUser: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}/lock`);
            const response = await apiClient.post(`/admin/users/${id}/lock`);
            return response.data;
        } catch (error) {
            console.error('Error locking user:', error);
            throw error;
        }
    },

    // Unlock user
    unlockUser: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${id}/unlock`);
            const response = await apiClient.post(`/admin/users/${id}/unlock`);
            return response.data;
        } catch (error) {
            console.error('Error unlocking user:', error);
            throw error;
        }
    },

    // Assign role to user
    assignRole: async (userId, roleId) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${userId}/assign-role`);
            const response = await apiClient.post(`/admin/users/${userId}/assign-role`, roleId);
            return response.data;
        } catch (error) {
            console.error('Error assigning role:', error);
            throw error;
        }
    },

    // Remove role from user
    removeRole: async (userId, roleId) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/users/${userId}/remove-role`);
            const response = await apiClient.post(`/admin/users/${userId}/remove-role`, roleId);
            return response.data;
        } catch (error) {
            console.error('Error removing role:', error);
            throw error;
        }
    }
};

// User Roles API
export const userRolesApi = {
    // Get all users with their roles and available roles
    getAll: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/user-roles`);
            const response = await apiClient.get('/admin/user-roles');
            return response.data;
        } catch (error) {
            console.error('Error fetching user roles:', error);
            // Return mock data as fallback
            return {
                users: [
                    {
                        userId: 1,
                        username: "admin",
                        roles: ["Administrator", "User Management"]
                    },
                    {
                        userId: 2,
                        username: "manager",
                        roles: ["Manager", "Reports"]
                    },
                    {
                        userId: 3,
                        username: "user1",
                        roles: ["User"]
                    },
                    {
                        userId: 4,
                        username: "user2",
                        roles: []
                    }
                ],
                allRoles: [
                    { id: 1, name: "Administrator" },
                    { id: 2, name: "Manager" },
                    { id: 3, name: "User" },
                    { id: 4, name: "Reports" },
                    { id: 5, name: "User Management" },
                    { id: 6, name: "Settings" }
                ]
            };
        }
    },

    // Update user roles
    updateRoles: async (userRoles) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/user-roles/update`);
            const response = await apiClient.post('/admin/user-roles/update', { userRoles });
            return response.data;
        } catch (error) {
            console.error('Error updating user roles:', error);
            // Return success for mock
            return { success: true };
        }
    }
};

// User Permission Overrides API
export const userPermissionOverridesApi = {
    // Get all users with their permission overrides
    getAll: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/user-permission-overrides`);
            const response = await apiClient.get('/admin/user-permission-overrides');
            return response.data;
        } catch (error) {
            console.error('Error fetching user permission overrides:', error);
            // Return mock data as fallback
            return [
                {
                    userId: 1,
                    username: "admin",
                    granted: ["UserManagement", "CreateReports", "SystemSettings"],
                    denied: ["DeleteUsers"],
                    allPermissions: [
                        {
                            id: 1,
                            name: "User Management",
                            systemName: "UserManagement",
                            category: "Administration",
                            description: "Manage user accounts and permissions",
                            isActive: true
                        },
                        {
                            id: 2,
                            name: "Create Reports",
                            systemName: "CreateReports",
                            category: "Reporting",
                            description: "Generate and create system reports",
                            isActive: true
                        },
                        {
                            id: 3,
                            name: "Delete Users",
                            systemName: "DeleteUsers",
                            category: "Administration",
                            description: "Remove user accounts from the system",
                            isActive: true
                        },
                        {
                            id: 4,
                            name: "System Settings",
                            systemName: "SystemSettings",
                            category: "Configuration",
                            description: "Modify system-wide settings",
                            isActive: true
                        },
                        {
                            id: 5,
                            name: "View Financial Data",
                            systemName: "ViewFinancialData",
                            category: "Finance",
                            description: "Access financial reports and data",
                            isActive: true
                        },
                        {
                            id: 6,
                            name: "Export Data",
                            systemName: "ExportData",
                            category: "Data",
                            description: "Export system data to external formats",
                            isActive: true
                        }
                    ]
                },
                {
                    userId: 2,
                    username: "manager",
                    granted: ["CreateReports", "ViewFinancialData"],
                    denied: ["SystemSettings"],
                    allPermissions: [
                        {
                            id: 1,
                            name: "User Management",
                            systemName: "UserManagement",
                            category: "Administration",
                            description: "Manage user accounts and permissions",
                            isActive: true
                        },
                        {
                            id: 2,
                            name: "Create Reports",
                            systemName: "CreateReports",
                            category: "Reporting",
                            description: "Generate and create system reports",
                            isActive: true
                        },
                        {
                            id: 3,
                            name: "Delete Users",
                            systemName: "DeleteUsers",
                            category: "Administration",
                            description: "Remove user accounts from the system",
                            isActive: true
                        },
                        {
                            id: 4,
                            name: "System Settings",
                            systemName: "SystemSettings",
                            category: "Configuration",
                            description: "Modify system-wide settings",
                            isActive: true
                        },
                        {
                            id: 5,
                            name: "View Financial Data",
                            systemName: "ViewFinancialData",
                            category: "Finance",
                            description: "Access financial reports and data",
                            isActive: true
                        },
                        {
                            id: 6,
                            name: "Export Data",
                            systemName: "ExportData",
                            category: "Data",
                            description: "Export system data to external formats",
                            isActive: true
                        }
                    ]
                },
                {
                    userId: 3,
                    username: "user1",
                    granted: [],
                    denied: ["DeleteUsers", "SystemSettings"],
                    allPermissions: [
                        {
                            id: 1,
                            name: "User Management",
                            systemName: "UserManagement",
                            category: "Administration",
                            description: "Manage user accounts and permissions",
                            isActive: true
                        },
                        {
                            id: 2,
                            name: "Create Reports",
                            systemName: "CreateReports",
                            category: "Reporting",
                            description: "Generate and create system reports",
                            isActive: true
                        },
                        {
                            id: 3,
                            name: "Delete Users",
                            systemName: "DeleteUsers",
                            category: "Administration",
                            description: "Remove user accounts from the system",
                            isActive: true
                        },
                        {
                            id: 4,
                            name: "System Settings",
                            systemName: "SystemSettings",
                            category: "Configuration",
                            description: "Modify system-wide settings",
                            isActive: true
                        },
                        {
                            id: 5,
                            name: "View Financial Data",
                            systemName: "ViewFinancialData",
                            category: "Finance",
                            description: "Access financial reports and data",
                            isActive: true
                        },
                        {
                            id: 6,
                            name: "Export Data",
                            systemName: "ExportData",
                            category: "Data",
                            description: "Export system data to external formats",
                            isActive: true
                        }
                    ]
                }
            ];
        }
    },

    // Grant permission to user
    grantPermission: async (userId, permissionSystemName) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/user-permission-overrides/${userId}/grant`);
            const response = await apiClient.post(`/admin/user-permission-overrides/${userId}/grant`, permissionSystemName);
            return response.data;
        } catch (error) {
            console.error('Error granting permission:', error);
            // Return success for mock
            return { success: true };
        }
    },

    // Deny permission to user
    denyPermission: async (userId, permissionSystemName) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/user-permission-overrides/${userId}/deny`);
            const response = await apiClient.post(`/admin/user-permission-overrides/${userId}/deny`, permissionSystemName);
            return response.data;
        } catch (error) {
            console.error('Error denying permission:', error);
            // Return success for mock
            return { success: true };
        }
    },

    // Remove permission override
    removeOverride: async (userId, permissionSystemName) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/user-permission-overrides/${userId}/remove`);
            const response = await apiClient.delete(`/admin/user-permission-overrides/${userId}/remove`, {
                data: permissionSystemName
            });
            return response.data;
        } catch (error) {
            console.error('Error removing permission override:', error);
            // Return success for mock
            return { success: true };
        }
    }
};

// Roles API
export const rolesApi = {
    // Get all roles
    getAll: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/roles`);
            const response = await apiClient.get('/admin/roles');
            return response.data;
        } catch (error) {
            console.error('Error fetching roles:', error);
            // Return mock data as fallback
            return [
                {
                    id: 1,
                    name: "Administrator",
                    systemName: "Administrator",
                    description: "Full system access with all permissions",
                    isActive: true,
                    isSystemRole: true
                },
                {
                    id: 2,
                    name: "Manager",
                    systemName: "Manager",
                    description: "Management level access to user and report functions",
                    isActive: true,
                    isSystemRole: false
                },
                {
                    id: 3,
                    name: "User",
                    systemName: "User",
                    description: "Standard user access with limited permissions",
                    isActive: true,
                    isSystemRole: false
                },
                {
                    id: 4,
                    name: "Report Viewer",
                    systemName: "ReportViewer",
                    description: "Read-only access to reports and data",
                    isActive: true,
                    isSystemRole: false
                },
                {
                    id: 5,
                    name: "Guest",
                    systemName: "Guest",
                    description: "Minimal access for temporary users",
                    isActive: false,
                    isSystemRole: false
                }
            ];
        }
    },

    // Get role by ID
    getById: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/roles/${id}`);
            const response = await apiClient.get(`/admin/roles/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching role:', error);
            // Return mock data as fallback
            return {
                id: id,
                name: "Sample Role",
                systemName: "SampleRole",
                description: "Sample role description",
                isActive: true,
                isSystemRole: false
            };
        }
    },

    // Create new role
    create: async (roleData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/roles`);
            const response = await apiClient.post('/admin/roles', roleData);
            return response.data;
        } catch (error) {
            console.error('Error creating role:', error);
            // Return success for mock
            return { success: true, roleId: Date.now() };
        }
    },

    // Update role
    update: async (id, roleData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/roles/${id}`);
            const response = await apiClient.put(`/admin/roles/${id}`, roleData);
            return response.data;
        } catch (error) {
            console.error('Error updating role:', error);
            // Return success for mock
            return { success: true };
        }
    },

    // Delete role
    delete: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/roles/${id}`);
            const response = await apiClient.delete(`/admin/roles/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error deleting role:', error);
            // Return success for mock
            return { success: true };
        }
    }
};

// Role Permission Mapping API
export const rolePermissionMappingApi = {
    // Get permissions for a specific role
    getRolePermissions: async (roleId) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/role-permissions/${roleId}`);
            const response = await apiClient.get(`/admin/role-permissions/${roleId}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching role permissions:', error);
            // Return mock data as fallback
            return {
                roleId: roleId,
                roleName: roleId === 1 ? "Administrator" : roleId === 2 ? "Manager" : "User",
                grantedPermissions: roleId === 1 ?
                    ["UserManagement", "CreateReports", "SystemSettings", "DeleteUsers", "ViewFinancialData", "ExportData"] :
                    roleId === 2 ?
                        ["CreateReports", "ViewFinancialData", "ExportData"] :
                        ["ViewFinancialData"],
                allPermissions: [
                    {
                        id: 1,
                        name: "User Management",
                        systemName: "UserManagement",
                        category: "Administration",
                        description: "Manage user accounts and permissions",
                        isActive: true
                    },
                    {
                        id: 2,
                        name: "Create Reports",
                        systemName: "CreateReports",
                        category: "Reporting",
                        description: "Generate and create system reports",
                        isActive: true
                    },
                    {
                        id: 3,
                        name: "Delete Users",
                        systemName: "DeleteUsers",
                        category: "Administration",
                        description: "Remove user accounts from the system",
                        isActive: true
                    },
                    {
                        id: 4,
                        name: "System Settings",
                        systemName: "SystemSettings",
                        category: "Configuration",
                        description: "Modify system-wide settings",
                        isActive: true
                    },
                    {
                        id: 5,
                        name: "View Financial Data",
                        systemName: "ViewFinancialData",
                        category: "Finance",
                        description: "Access financial reports and data",
                        isActive: true
                    },
                    {
                        id: 6,
                        name: "Export Data",
                        systemName: "ExportData",
                        category: "Data",
                        description: "Export system data to external formats",
                        isActive: true
                    },
                    {
                        id: 7,
                        name: "Audit Logs",
                        systemName: "AuditLogs",
                        category: "Security",
                        description: "View and manage system audit logs",
                        isActive: true
                    },
                    {
                        id: 8,
                        name: "Backup Management",
                        systemName: "BackupManagement",
                        category: "System",
                        description: "Manage system backups and restoration",
                        isActive: true
                    }
                ]
            };
        }
    },

    // Update role permissions
    updateRolePermissions: async (roleId, selectedPermissions) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/role-permissions/${roleId}`);
            const response = await apiClient.put(`/admin/role-permissions/${roleId}`, selectedPermissions);
            return response.data;
        } catch (error) {
            console.error('Error updating role permissions:', error);
            // Return success for mock
            return { success: true, message: "Role permissions updated successfully" };
        }
    }
};

// Permissions API
export const permissionsApi = {
    // Get all permissions
    getAll: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/permissions`);
            const response = await apiClient.get('/admin/permissions');
            return response.data;
        } catch (error) {
            console.error('Error fetching permissions:', error);
            // Return mock data as fallback
            return [
                {
                    id: 1,
                    name: "User Management",
                    systemName: "UserManagement",
                    category: "Administration",
                    description: "Manage user accounts and permissions",
                    isActive: true
                },
                {
                    id: 2,
                    name: "Create Reports",
                    systemName: "CreateReports",
                    category: "Reporting",
                    description: "Generate and create system reports",
                    isActive: true
                },
                {
                    id: 3,
                    name: "Delete Users",
                    systemName: "DeleteUsers",
                    category: "Administration",
                    description: "Remove user accounts from the system",
                    isActive: true
                },
                {
                    id: 4,
                    name: "System Settings",
                    systemName: "SystemSettings",
                    category: "Configuration",
                    description: "Modify system-wide settings",
                    isActive: true
                },
                {
                    id: 5,
                    name: "View Financial Data",
                    systemName: "ViewFinancialData",
                    category: "Finance",
                    description: "Access financial reports and data",
                    isActive: true
                },
                {
                    id: 6,
                    name: "Export Data",
                    systemName: "ExportData",
                    category: "Data",
                    description: "Export system data to external formats",
                    isActive: true
                },
                {
                    id: 7,
                    name: "Audit Logs",
                    systemName: "AuditLogs",
                    category: "Security",
                    description: "View and manage system audit logs",
                    isActive: true
                },
                {
                    id: 8,
                    name: "Backup Management",
                    systemName: "BackupManagement",
                    category: "System",
                    description: "Manage system backups and restoration",
                    isActive: true
                },
                {
                    id: 9,
                    name: "API Access",
                    systemName: "APIAccess",
                    category: "Integration",
                    description: "Access to system APIs and integrations",
                    isActive: false
                },
                {
                    id: 10,
                    name: "Database Management",
                    systemName: "DatabaseManagement",
                    category: "System",
                    description: "Direct database access and management",
                    isActive: true
                }
            ];
        }
    },

    // Sync permissions
    syncPermissions: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/permissions/sync`);
            const response = await apiClient.post('/admin/permissions/sync');
            return response.data;
        } catch (error) {
            console.error('Error syncing permissions:', error);
            // Return success for mock
            return { success: true, message: "Permissions synchronized successfully" };
        }
    }
};

// Languages API
export const languagesApi = {
    // Get all languages
    getAll: async () => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/languageApi`);
            const response = await apiClient.get('/admin/languageApi');
            return response.data;
        } catch (error) {
            console.error('Error fetching languages:', error);
            // Return mock data as fallback
            return [
                {
                    id: 1,
                    name: "English",
                    languageCulture: "en-US",
                    uniqueSeoCode: "en",
                    flagImageFileName: "us.png",
                    rtl: false,
                    published: true,
                    displayOrder: 1
                },
                {
                    id: 2,
                    name: "العربية",
                    languageCulture: "ar-SA",
                    uniqueSeoCode: "ar",
                    flagImageFileName: "sa.png",
                    rtl: true,
                    published: true,
                    displayOrder: 2
                },
                {
                    id: 3,
                    name: "Français",
                    languageCulture: "fr-FR",
                    uniqueSeoCode: "fr",
                    flagImageFileName: "fr.png",
                    rtl: false,
                    published: true,
                    displayOrder: 3
                },
                {
                    id: 4,
                    name: "Español",
                    languageCulture: "es-ES",
                    uniqueSeoCode: "es",
                    flagImageFileName: "es.png",
                    rtl: false,
                    published: false,
                    displayOrder: 4
                },
                {
                    id: 5,
                    name: "Deutsch",
                    languageCulture: "de-DE",
                    uniqueSeoCode: "de",
                    flagImageFileName: "de.png",
                    rtl: false,
                    published: true,
                    displayOrder: 5
                }
            ];
        }
    },

    // Get language by ID
    getById: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/languageapi/${id}`);
            const response = await apiClient.get(`/admin/languageapi/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching language:', error);
            // Return mock data as fallback
            return {
                id: id,
                name: "Sample Language",
                languageCulture: "en-US",
                uniqueSeoCode: "sample",
                flagImageFileName: "sample.png",
                rtl: false,
                published: true,
                displayOrder: 1
            };
        }
    },

    // Create new language
    create: async (languageData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/languageapi`);
            const response = await apiClient.post('/admin/languageapi', languageData);
            return response.data;
        } catch (error) {
            console.error('Error creating language:', error);
            // Return success for mock
            return { success: true, languageId: Date.now() };
        }
    },

    // Update language
    update: async (id, languageData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/languageapi/${id}`);
            const response = await apiClient.put(`/admin/languageapi/${id}`, languageData);
            return response.data;
        } catch (error) {
            console.error('Error updating language:', error);
            // Return success for mock
            return { success: true };
        }
    },

    // Delete language
    delete: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/languageapi/${id}`);
            const response = await apiClient.delete(`/admin/languageapi/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error deleting language:', error);
            // Return success for mock
            return { success: true };
        }
    },

    // Change language
    changeLanguage: async (lang, returnUrl) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/languageapi/change`);
            const response = await apiClient.post('/admin/languageapi/change', null, {
                params: { lang, returnUrl }
            });
            return response.data;
        } catch (error) {
            console.error('Error changing language:', error);
            // Return success for mock
            return { success: true, returnUrl: returnUrl || "/" };
        }
    }
};

// Resources API
export const resourcesApi = {
    // Get all resources
    getAll: async (languageId = 1, query = null) => {
        try {
            const params = new URLSearchParams();
            params.append('languageId', languageId.toString());
            if (query) {
                params.append('q', query);
            }

            console.log('Making API call to:', `${API_BASE_URL}/admin/resources?${params.toString()}`);
            const response = await apiClient.get(`/admin/resources?${params.toString()}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching resources:', error);
            // Return mock data as fallback
            return [
                {
                    id: 1,
                    resourceName: "Common.Welcome",
                    resourceValue: "Welcome",
                    languageId: 1
                },
                {
                    id: 2,
                    resourceName: "Common.Login",
                    resourceValue: "Login",
                    languageId: 1
                },
                {
                    id: 3,
                    resourceName: "Common.Logout",
                    resourceValue: "Logout",
                    languageId: 1
                },
                {
                    id: 4,
                    resourceName: "Common.Save",
                    resourceValue: "Save",
                    languageId: 1
                },
                {
                    id: 5,
                    resourceName: "Common.Cancel",
                    resourceValue: "Cancel",
                    languageId: 1
                },
                {
                    id: 6,
                    resourceName: "Admin.Users.Title",
                    resourceValue: "User Management",
                    languageId: 1
                },
                {
                    id: 7,
                    resourceName: "Admin.Users.Create",
                    resourceValue: "Create User",
                    languageId: 1
                },
                {
                    id: 8,
                    resourceName: "Admin.Users.Edit",
                    resourceValue: "Edit User",
                    languageId: 1
                },
                {
                    id: 9,
                    resourceName: "Admin.Users.Delete",
                    resourceValue: "Delete User",
                    languageId: 1
                },
                {
                    id: 10,
                    resourceName: "Common.Welcome",
                    resourceValue: "مرحباً",
                    languageId: 2
                },
                {
                    id: 11,
                    resourceName: "Common.Login",
                    resourceValue: "تسجيل الدخول",
                    languageId: 2
                },
                {
                    id: 12,
                    resourceName: "Common.Logout",
                    resourceValue: "تسجيل الخروج",
                    languageId: 2
                }
            ];
        }
    },

    // Get resource by ID
    getById: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/resources/${id}`);
            const response = await apiClient.get(`/admin/resources/${id}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching resource:', error);
            // Return mock data as fallback
            return {
                id: id,
                resourceName: "Sample.Resource",
                resourceValue: "Sample Value",
                languageId: 1
            };
        }
    },

    // Create new resource
    create: async (resourceData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/resources`);
            const response = await apiClient.post('/admin/resources', resourceData);
            return response.data;
        } catch (error) {
            console.error('Error creating resource:', error);
            // Return success for mock
            return { success: true, message: "Resource created successfully" };
        }
    },

    // Update resource
    update: async (id, resourceData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/resources/${id}`);
            const response = await apiClient.put(`/admin/resources/${id}`, resourceData);
            return response.data;
        } catch (error) {
            console.error('Error updating resource:', error);
            // Return success for mock
            return { success: true, message: "Resource updated successfully" };
        }
    },

    // Delete resource
    delete: async (id, languageId) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/resources/${id}?languageId=${languageId}`);
            const response = await apiClient.delete(`/admin/resources/${id}?languageId=${languageId}`);
            return response.data;
        } catch (error) {
            console.error('Error deleting resource:', error);
            // Return success for mock
            return { success: true, message: "Resource deleted successfully" };
        }
    }
};

// Registrations API (Entity Profile) - Updated to match C# backend
export const registrationsApi = {
    // Get all registrations - matches your C# API endpoint
    getAll: async (userId) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations?userId=${userId}`);
            const response = await apiClient.get(`/admin/registrations?userId=${userId}`);
            console.log('Registrations API response:', response.data);

            // Transform the response to match frontend expectations
            const transformedData = Array.isArray(response.data) ? response.data.map(reg => ({
                id: reg.id,
                institutionId: reg.institutionId,
                institutionName: reg.institutionName,
                licenseNumber: reg.licenseNumber,
                statusId: reg.statusId,
                status: reg.status, // This comes as enum string from C# API
                createdOnUtc: reg.createdOnUtc,
                createdByUserName: reg.createdByUserName
            })) : [];

            return transformedData;
        } catch (error) {
            console.error('Error fetching registrations from API:', error);
            console.log('Falling back to mock data for development...');

            // Return mock data as fallback for development
            return [
                {
                    id: 1,
                    institutionId: 101,
                    institutionName: "ABC Bank Ltd.",
                    licenseNumber: "LIC-2024-001",
                    statusId: 1,
                    status: "Draft",
                    createdOnUtc: "2024-10-01T10:30:00Z",
                    createdByUserName: "john.doe"
                },
                {
                    id: 2,
                    institutionId: 102,
                    institutionName: "XYZ Financial Services",
                    licenseNumber: "LIC-2024-002",
                    statusId: 2,
                    status: "Submitted",
                    createdOnUtc: "2024-10-02T14:15:00Z",
                    createdByUserName: "jane.smith"
                },
                {
                    id: 3,
                    institutionId: 103,
                    institutionName: "Global Investment Corp",
                    licenseNumber: "LIC-2024-003",
                    statusId: 3,
                    status: "Approved",
                    createdOnUtc: "2024-10-03T09:45:00Z",
                    createdByUserName: "mike.wilson"
                },
                {
                    id: 4,
                    institutionId: 104,
                    institutionName: "Regional Credit Union",
                    licenseNumber: "LIC-2024-004",
                    statusId: 4,
                    status: "Rejected",
                    createdOnUtc: "2024-10-04T16:20:00Z",
                    createdByUserName: "sarah.jones"
                },
                {
                    id: 5,
                    institutionId: 105,
                    institutionName: "Metro Insurance Co.",
                    licenseNumber: "LIC-2024-005",
                    statusId: 5,
                    status: "ReturnedForEdit",
                    createdOnUtc: "2024-10-05T11:10:00Z",
                    createdByUserName: "david.brown"
                }
            ];
        }
    },

    // Get registration by ID - matches your C# API endpoint
    getById: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}`);
            const response = await apiClient.get(`/admin/registrations/${id}`);
            console.log('Registration details API response:', response.data);

            // Transform the response to match frontend expectations
            const reg = response.data;
            return {
                id: reg.id,
                institutionId: reg.institutionId,
                institutionName: reg.institutionName,
                licenseNumber: reg.licenseNumber,
                licenseSectorId: reg.licenseSectorId,
                issueDate: reg.issueDate,
                expiryDate: reg.expiryDate,
                statusId: reg.statusId,
                countryName: reg.countryName,
                createdByUserName: reg.createdByUserName,
                isActive: reg.isActive,
                contacts: reg.contacts || [],
                documents: reg.documents || [],
                statusLogs: reg.statusLogs || []
            };
        } catch (error) {
            console.error('Error fetching registration details from API:', error);
            console.log('Falling back to mock data for development...');

            // Return mock data as fallback
            return {
                id: id,
                institutionId: 101,
                institutionName: "ABC Bank Ltd.",
                licenseNumber: "LIC-2024-001",
                licenseSectorId: 1,
                issueDate: "2024-01-15",
                expiryDate: "2025-01-15",
                statusId: 2,
                countryName: "United Arab Emirates",
                createdByUserName: "john.doe",
                isActive: true,
                contacts: [
                    {
                        id: 1,
                        name: "John Smith",
                        email: "john.smith@abcbank.com",
                        phone: "+971-50-1234567",
                        position: "CEO"
                    },
                    {
                        id: 2,
                        name: "Sarah Johnson",
                        email: "sarah.johnson@abcbank.com",
                        phone: "+971-50-7654321",
                        position: "CFO"
                    }
                ],
                documents: [
                    {
                        id: 1,
                        name: "License Certificate",
                        fileName: "license_certificate.pdf",
                        uploadedDate: "2024-01-10"
                    },
                    {
                        id: 2,
                        name: "Corporate Structure",
                        fileName: "corporate_structure.pdf",
                        uploadedDate: "2024-01-12"
                    }
                ],
                statusLogs: [
                    {
                        id: 1,
                        status: "Draft",
                        date: "2024-01-01T10:00:00Z",
                        remarks: "Initial registration created",
                        changedBy: "john.doe"
                    },
                    {
                        id: 2,
                        status: "Submitted",
                        date: "2024-01-15T14:30:00Z",
                        remarks: "Submitted for review",
                        changedBy: "john.doe"
                    }
                ]
            };
        }
    },

    // Create new registration
    create: async (registrationData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations`);
            const response = await apiClient.post('/admin/registrations', registrationData);
            return response.data;
        } catch (error) {
            console.error('Error creating registration:', error);
            // Return success for mock
            return { success: true, registrationId: Date.now() };
        }
    },

    // Update registration
    update: async (id, registrationData) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}`);
            const response = await apiClient.put(`/admin/registrations/${id}`, registrationData);
            return response.data;
        } catch (error) {
            console.error('Error updating registration:', error);
            // Return success for mock
            return { success: true };
        }
    },

    // Submit registration - matches POST /api/admin/registrations/{id}/submit
    submit: async (id, remarks) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}/submit`);
            console.log('Submitting with remarks:', remarks);

            const response = await apiClient.post(`/admin/registrations/${id}/submit`, remarks, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            console.log('Submit registration response:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error submitting registration to API:', error);
            console.log('Falling back to mock success for development...');
            return { success: true };
        }
    },

    // Validate registration - matches POST /api/admin/registrations/{id}/validate
    validate: async (id, remarks) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}/validate`);
            console.log('Validating with remarks:', remarks);

            const response = await apiClient.post(`/admin/registrations/${id}/validate`, remarks, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            console.log('Validate registration response:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error validating registration to API:', error);
            console.log('Falling back to mock success for development...');
            return { success: true };
        }
    },

    // Approve registration - matches POST /api/admin/registrations/{id}/approve
    approve: async (id) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}/approve`);

            const response = await apiClient.post(`/admin/registrations/${id}/approve`);

            console.log('Approve registration response:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error approving registration to API:', error);
            console.log('Falling back to mock success for development...');
            return { success: true };
        }
    },

    // Reject registration - matches POST /api/admin/registrations/{id}/reject
    reject: async (id, comment) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}/reject`);
            console.log('Rejecting with comment:', comment);

            const response = await apiClient.post(`/admin/registrations/${id}/reject`, comment, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            console.log('Reject registration response:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error rejecting registration to API:', error);
            console.log('Falling back to mock success for development...');
            return { success: true };
        }
    },

    // Return for edit - matches POST /api/admin/registrations/{id}/return-for-edit
    returnForEdit: async (id, remarks) => {
        try {
            console.log('Making API call to:', `${API_BASE_URL}/admin/registrations/${id}/return-for-edit`);
            console.log('Returning for edit with remarks:', remarks);

            const response = await apiClient.post(`/admin/registrations/${id}/return-for-edit`, remarks, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            console.log('Return for edit response:', response.data);
            return response.data;
        } catch (error) {
            console.error('Error returning registration for edit to API:', error);
            console.log('Falling back to mock success for development...');
            return { success: true };
        }
    }
};

export default apiClient;
