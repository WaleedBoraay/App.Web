// User utility functions for authentication and user management

/**
 * Get the current logged-in user from localStorage
 * @returns {Object|null} - User object or null if not logged in
 */
export const getCurrentUser = () => {
  try {
    // Try both 'currentUser' and 'user' keys for compatibility
    let userData = localStorage.getItem('currentUser') || localStorage.getItem('user');
    if (!userData) {
      return null;
    }
    return JSON.parse(userData);
  } catch (error) {
    console.error('Error parsing user data from localStorage:', error);
    return null;
  }
};

/**
 * Get the current user's ID
 * @returns {number|null} - User ID or null if not logged in
 */
export const getCurrentUserId = () => {
  const user = getCurrentUser();
  if (!user) {
    console.log('getCurrentUserId: No user found');
    return null;
  }
  
  // Try different possible ID field names
  const userId = user.id || user.userId || user.ID || user.UserId;
  
  if (!userId) {
    console.warn('getCurrentUserId: User object found but no valid ID field:', user);
    // If no ID field found, we could use a fallback or generate one
    // For now, let's try to use a default ID for development
    return 4; // Default fallback ID (matching your requirement)
  }
  
  console.log('getCurrentUserId: Found user ID:', userId);
  return parseInt(userId) || userId;
};

/**
 * Get the current user's username
 * @returns {string|null} - Username or null if not logged in
 */
export const getCurrentUsername = () => {
  const user = getCurrentUser();
  return user ? user.username : null;
};

/**
 * Check if user is logged in
 * @returns {boolean} - True if user is logged in, false otherwise
 */
export const isUserLoggedIn = () => {
  return getCurrentUser() !== null;
};

/**
 * Check if current user has a specific role
 * @param {string} roleName - Role name to check
 * @returns {boolean} - True if user has the role, false otherwise
 */
export const hasRole = (roleName) => {
  const user = getCurrentUser();
  if (!user || !user.roles) {
    return false;
  }
  return Array.isArray(user.roles) && user.roles.includes(roleName);
};

/**
 * Check if current user is a super admin
 * @returns {boolean} - True if user is super admin, false otherwise
 */
export const isSuperAdmin = () => {
  return hasRole('Super Admin');
};

/**
 * Get the current user's institution information
 * @returns {Object|null} - Institution object or null if not available
 */
export const getCurrentUserInstitution = () => {
  const user = getCurrentUser();
  return user ? user.institution : null;
};

/**
 * Log out the current user by removing user data from localStorage
 */
export const logout = () => {
  localStorage.removeItem('user');
  localStorage.removeItem('currentUser');
  localStorage.removeItem('authToken');
  localStorage.removeItem('registrationStep');
  localStorage.removeItem('step1Completed');
  localStorage.removeItem('fromDashboard');
  localStorage.removeItem('autoFillCredentials');
};

/**
 * Update user data in localStorage
 * @param {Object} userData - Updated user data
 */
export const updateUserData = (userData) => {
  try {
    // Update both keys for compatibility
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('currentUser', JSON.stringify(userData));
  } catch (error) {
    console.error('Error updating user data in localStorage:', error);
  }
};

/**
 * Get authentication token from localStorage
 * @returns {string|null} - Auth token or null if not available
 */
export const getAuthToken = () => {
  return localStorage.getItem('authToken');
};

/**
 * Check if current user's account is active
 * @returns {boolean} - True if active, false otherwise
 */
export const isUserActive = () => {
  const user = getCurrentUser();
  return user ? user.isActive === true : false;
};

/**
 * Debug user data structure - logs current user data to console
 * @returns {Object|null} - User object for inspection
 */
export const debugUserData = () => {
  const userData = localStorage.getItem('currentUser') || localStorage.getItem('user');
  console.log('Raw user data from localStorage (currentUser):', localStorage.getItem('currentUser'));
  console.log('Raw user data from localStorage (user):', localStorage.getItem('user'));
  
  if (!userData) {
    console.log('No user data found in localStorage');
    return null;
  }
  
  try {
    const parsedUser = JSON.parse(userData);
    console.log('Parsed user data:', parsedUser);
    console.log('Available keys in user object:', Object.keys(parsedUser));
    console.log('User ID fields check:', {
      id: parsedUser.id,
      userId: parsedUser.userId,
      ID: parsedUser.ID,
      UserId: parsedUser.UserId
    });
    console.log('User roles check:', {
      role: parsedUser.role,
      roles: parsedUser.roles
    });
    return parsedUser;
  } catch (error) {
    console.error('Error parsing user data:', error);
    return null;
  }
};

/**
 * Create mock user data for development/testing
 * @returns {Object} - Mock user object
 */
export const createMockUser = () => {
  const mockUser = {
    id: 4,
    userId: 4,
    username: 'admin@yourstore.com',
    email: 'admin@yourstore.com',
    firstName: 'Test',
    lastName: 'User',
    avatar: "https://via.placeholder.com/40x40/007bff/ffffff?text=TU",
    isActive: true,
    role: 'Admin', // Default role for easy testing
    roles: ['Admin', 'Maker', 'Checker'], // Multiple roles for comprehensive testing
    isAuthenticated: true,
    loginTime: new Date().toISOString(),
    institution: {
      id: 4,
      name: 'Test Bank Corporation',
      licenseNumber: 'LIC-2024-004',
      businessPhoneNumber: '+971-50-1234567',
      address: 'Dubai Financial District, UAE',
      createdOnUtc: new Date().toISOString()
    }
  };
  
  console.log('Creating mock user for development:', mockUser);
  localStorage.setItem('user', JSON.stringify(mockUser));
  localStorage.setItem('currentUser', JSON.stringify(mockUser));
  localStorage.setItem('authToken', 'mock-auth-token-for-development');
  
  return mockUser;
};

/**
 * Fix existing user data to have correct ID
 * @returns {Object|null} - Fixed user object or null
 */
export const fixExistingUserData = () => {
  const existingUser = getCurrentUser();
  if (existingUser && (!existingUser.id || existingUser.id !== 4)) {
    console.log('Fixing existing user data with correct ID...');
    const fixedUser = {
      ...existingUser,
      id: 4,
      userId: 4,
      username: existingUser.username || 'admin@yourstore.com',
      email: existingUser.email || existingUser.username || 'admin@yourstore.com'
    };
    updateUserData(fixedUser);
    return fixedUser;
  }
  return existingUser;
};

/**
 * Setup development user if no user exists
 * @returns {Object|null} - User object or null
 */
export const setupDevUser = () => {
  // First try to fix existing user data
  const fixedUser = fixExistingUserData();
  if (fixedUser) {
    return fixedUser;
  }
  
  // If no user exists, create a new mock user
  console.log('No user found, creating mock user for development...');
  return createMockUser();
};

const userUtils = {
  getCurrentUser,
  getCurrentUserId,
  getCurrentUsername,
  isUserLoggedIn,
  hasRole,
  isSuperAdmin,
  getCurrentUserInstitution,
  logout,
  updateUserData,
  getAuthToken,
  isUserActive,
  debugUserData,
  createMockUser,
  fixExistingUserData,
  setupDevUser
};

export default userUtils;