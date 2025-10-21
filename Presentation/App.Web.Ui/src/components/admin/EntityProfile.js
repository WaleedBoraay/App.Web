import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import Modal from 'react-modal';
import { registrationsApi } from '../../services/apiService';
import { getCurrentUserId, debugUserData, setupDevUser } from '../../utils/userUtils';
import UserDebugPanel from './UserDebugPanel';

// Set app element for Modal accessibility
if (typeof window !== 'undefined') {
  Modal.setAppElement(document.getElementById('root') || document.body);
}

const EntityProfile = () => {
  const [registrations, setRegistrations] = useState([]);
  const [selectedRegistration, setSelectedRegistration] = useState(null);
  const [loading, setLoading] = useState(false);
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [actionModalOpen, setActionModalOpen] = useState(false);
  const [actionType, setActionType] = useState('');
  const [actionRemarks, setActionRemarks] = useState('');
  const [filterStatus, setFilterStatus] = useState('All');
  const [searchQuery, setSearchQuery] = useState('');
  const [sortField, setSortField] = useState('createdOnUtc');
  const [sortDirection, setSortDirection] = useState('desc');
  const [currentUser, setCurrentUser] = useState(null);
  const [currentUserId, setCurrentUserId] = useState(null);

  // Status options matching C# RegistrationStatus enum
  const statusOptions = ['All', 'Draft', 'Submitted', 'Approved', 'Rejected', 'ReturnedForEdit'];
  const statusColors = {
    Draft: '#6c757d',
    Submitted: '#007bff',
    Approved: '#28a745',
    Rejected: '#dc3545',
    ReturnedForEdit: '#ffc107',
    // Additional statuses from C# enum if needed
    FinalSubmission: '#17a2b8',
    Archived: '#868e96'
  };

  // Initialize current user on component mount
  useEffect(() => {
    console.log('EntityProfile: Initializing user authentication...');
    
    // Debug user data structure
    debugUserData();
    
    // Setup development user if needed
    const user = setupDevUser();
    
    if (user) {
      setCurrentUser(user);
      const userId = getCurrentUserId();
      
      console.log('EntityProfile: User found', { user, userId });
      
      if (userId) {
        setCurrentUserId(userId);
        console.log('EntityProfile: Current user loaded successfully', { 
          username: user.username, 
          userId, 
          email: user.email 
        });
        toast.success(`Welcome, ${user.username || 'User'}!`);
      } else {
        console.error('EntityProfile: No valid user ID found in user data', user);
        toast.error('User data is incomplete. Using fallback ID for development.');
        // Set fallback ID for development (matching your requirement)
        setCurrentUserId(4);
      }
    } else {
      console.error('EntityProfile: Failed to setup user');
      toast.error('Failed to initialize user. Please refresh the page.');
    }
  }, []);

  const loadRegistrations = useCallback(async () => {
    if (!currentUserId) {
      console.log('No current user ID available yet');
      return;
    }
    
    setLoading(true);
    try {
      console.log('Loading registrations for user ID:', currentUserId);
      const data = await registrationsApi.getAll(currentUserId);
      setRegistrations(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error('Error loading registrations:', error);
      toast.error('Failed to load registrations');
    } finally {
      setLoading(false);
    }
  }, [currentUserId]);

  useEffect(() => {
    if (currentUserId) {
      loadRegistrations();
    }
  }, [loadRegistrations, currentUserId]);

  const handleViewDetails = async (registration) => {
    setLoading(true);
    try {
      const details = await registrationsApi.getById(registration.id);
      setSelectedRegistration(details);
      setDetailModalOpen(true);
    } catch (error) {
      console.error('Error loading registration details:', error);
      toast.error('Failed to load registration details');
    } finally {
      setLoading(false);
    }
  };

  const handleAction = (registration, action) => {
    setSelectedRegistration(registration);
    setActionType(action);
    setActionRemarks('');
    setActionModalOpen(true);
  };

  const executeAction = async () => {
    if (!selectedRegistration) return;

    setLoading(true);
    try {
      const id = selectedRegistration.id;
      
      switch (actionType) {
        case 'submit':
          await registrationsApi.submit(id, actionRemarks);
          toast.success('Registration submitted successfully');
          break;
        case 'validate':
          await registrationsApi.validate(id, actionRemarks);
          toast.success('Registration validated successfully');
          break;
        case 'approve':
          await registrationsApi.approve(id);
          toast.success('Registration approved successfully');
          break;
        case 'reject':
          await registrationsApi.reject(id, actionRemarks);
          toast.success('Registration rejected successfully');
          break;
        case 'returnForEdit':
          await registrationsApi.returnForEdit(id, actionRemarks);
          toast.success('Registration returned for edit successfully');
          break;
        default:
          break;
      }

      setActionModalOpen(false);
      loadRegistrations();
    } catch (error) {
      console.error(`Error ${actionType} registration:`, error);
      toast.error(`Failed to ${actionType} registration`);
    } finally {
      setLoading(false);
    }
  };

  const handleSort = (field) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getSortIcon = (field) => {
    if (sortField !== field) return '↕️';
    return sortDirection === 'asc' ? '↑' : '↓';
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getActionButtons = (registration) => {
    const buttons = [];
    const status = registration.status;
    
    // TODO: Get actual user roles from currentUser.roles
    // For now, show all actions (will be filtered by backend API based on actual user roles)
    const userRoles = currentUser?.roles || [];
    const isMaker = userRoles.includes('Maker') || userRoles.includes('Admin');
    const isChecker = userRoles.includes('Checker') || userRoles.includes('Admin');
    
    // Makers can submit Draft and ReturnedForEdit registrations
    if ((status === 'Draft' || status === 'ReturnedForEdit') && isMaker) {
      buttons.push(
        <button
          key="submit"
          onClick={() => handleAction(registration, 'submit')}
          style={{
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            padding: '4px 8px',
            borderRadius: '3px',
            cursor: 'pointer',
            fontSize: '11px',
            margin: '2px'
          }}
          title="Submit for review"
        >
          📤 Submit
        </button>
      );
    }

    // Checkers can validate, approve, reject, or return Submitted registrations
    if (status === 'Submitted' && isChecker) {
      buttons.push(
        <button
          key="validate"
          onClick={() => handleAction(registration, 'validate')}
          style={{
            backgroundColor: '#28a745',
            color: 'white',
            border: 'none',
            padding: '4px 8px',
            borderRadius: '3px',
            cursor: 'pointer',
            fontSize: '11px',
            margin: '2px'
          }}
          title="Validate and approve registration"
        >
          ✅ Validate
        </button>
      );
      
      buttons.push(
        <button
          key="approve"
          onClick={() => handleAction(registration, 'approve')}
          style={{
            backgroundColor: '#198754',
            color: 'white',
            border: 'none',
            padding: '4px 8px',
            borderRadius: '3px',
            cursor: 'pointer',
            fontSize: '11px',
            margin: '2px'
          }}
          title="Approve registration"
        >
          ✅ Approve
        </button>
      );
      
      buttons.push(
        <button
          key="returnForEdit"
          onClick={() => handleAction(registration, 'returnForEdit')}
          style={{
            backgroundColor: '#ffc107',
            color: '#000',
            border: 'none',
            padding: '4px 8px',
            borderRadius: '3px',
            cursor: 'pointer',
            fontSize: '11px',
            margin: '2px'
          }}
          title="Return for corrections"
        >
          📝 Return
        </button>
      );
      
      buttons.push(
        <button
          key="reject"
          onClick={() => handleAction(registration, 'reject')}
          style={{
            backgroundColor: '#dc3545',
            color: 'white',
            border: 'none',
            padding: '4px 8px',
            borderRadius: '3px',
            cursor: 'pointer',
            fontSize: '11px',
            margin: '2px'
          }}
          title="Reject registration"
        >
          ❌ Reject
        </button>
      );
    }

    // View details is available to everyone
    buttons.push(
      <button
        key="view"
        onClick={() => handleViewDetails(registration)}
        style={{
          backgroundColor: '#17a2b8',
          color: 'white',
          border: 'none',
          padding: '4px 8px',
          borderRadius: '3px',
          cursor: 'pointer',
          fontSize: '11px',
          margin: '2px'
        }}
        title="View registration details"
      >
        👁️ View
      </button>
    );

    return buttons;
  };

  const filteredRegistrations = registrations
    .filter(reg => {
      const matchesStatus = filterStatus === 'All' || reg.status === filterStatus;
      const matchesSearch = !searchQuery || 
        reg.institutionName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        reg.licenseNumber?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        reg.createdByUserName?.toLowerCase().includes(searchQuery.toLowerCase());
      
      return matchesStatus && matchesSearch;
    })
    .sort((a, b) => {
      const aVal = a[sortField] || '';
      const bVal = b[sortField] || '';
      
      if (sortField === 'createdOnUtc') {
        const aDate = new Date(aVal);
        const bDate = new Date(bVal);
        return sortDirection === 'asc' 
          ? aDate.getTime() - bDate.getTime()
          : bDate.getTime() - aDate.getTime();
      }
      
      if (typeof aVal === 'string') {
        return sortDirection === 'asc' 
          ? aVal.localeCompare(bVal)
          : bVal.localeCompare(aVal);
      }
      
      return sortDirection === 'asc' 
        ? (aVal < bVal ? -1 : aVal > bVal ? 1 : 0)
        : (bVal < aVal ? -1 : bVal > aVal ? 1 : 0);
    });

  const modalStyles = {
    overlay: {
      backgroundColor: 'rgba(0, 0, 0, 0.6)',
      zIndex: 1000,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center'
    },
    content: {
      position: 'relative',
      top: 'auto',
      left: 'auto',
      right: 'auto',
      bottom: 'auto',
      margin: '20px',
      padding: '0',
      border: 'none',
      borderRadius: '12px',
      maxWidth: '900px',
      width: '100%',
      maxHeight: '90vh',
      overflow: 'auto',
      boxShadow: '0 10px 40px rgba(0,0,0,0.15)',
      transform: 'none'
    }
  };

  // Show loading state while checking for user
  if (!currentUser && !loading) {
    return (
      <div style={{ 
        padding: '40px', 
        textAlign: 'center', 
        color: '#666',
        fontSize: '16px'
      }}>
        <div style={{ marginBottom: '16px' }}>
          <i style={{ fontSize: '48px', color: '#dc3545' }}>🔒</i>
        </div>
        <h3 style={{ color: '#dc3545', margin: '0 0 8px 0' }}>Authentication Required</h3>
        <p style={{ margin: 0 }}>
          Please log in to access the Entity Registration Management system.
        </p>
      </div>
    );
  }

  return (
    <>
      {/* Development Debug Panel - Remove in production */}
      {process.env.NODE_ENV === 'development' && <UserDebugPanel />}
      
      {/* Add CSS animation for loading spinner */}
      <style>
        {`
          @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
          }
          
          /* Responsive improvements */
          @media (max-width: 768px) {
            .entity-filters {
              flex-direction: column !important;
              align-items: stretch !important;
            }
            
            .entity-filter-item {
              width: 100% !important;
              margin-bottom: 12px !important;
            }
            
            .entity-stats-grid {
              grid-template-columns: 1fr !important;
            }
            
            .entity-header {
              flex-direction: column !important;
              text-align: center !important;
            }
            
            .entity-table-actions {
              flex-wrap: wrap !important;
              gap: 4px !important;
            }
          }
          
          @media (max-width: 480px) {
            .entity-modal-content {
              margin: 10px !important;
              width: calc(100% - 20px) !important;
            }
          }
        `}
      </style>
      <div style={{ 
        minHeight: '100vh',
        backgroundColor: '#f8f9fa',
        padding: '0'
      }}>
        {/* Main Container - Full Width Fluid */}
        <div style={{
          width: '100%',
          maxWidth: 'none',
          margin: '0',
          padding: '16px 20px',
          boxSizing: 'border-box'
        }}>
          {/* Header */}
          <div className="entity-header" style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center', 
            marginBottom: '24px',
            flexWrap: 'wrap',
            gap: '12px',
            backgroundColor: 'white',
            padding: '20px 24px',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            border: '1px solid #e9ecef'
          }}>
        <div>
          <h2 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '600' }}>
            Entity Registration Management
          </h2>
          <p style={{ margin: '0', color: '#666', fontSize: '14px' }}>
            Manage institution registrations and entity registration process
          </p>
        </div>
        {currentUser && (
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            padding: '12px 16px',
            backgroundColor: '#f8f9fa',
            borderRadius: '8px',
            border: '1px solid #e9ecef'
          }}>
            <div style={{
              width: '40px',
              height: '40px',
              backgroundColor: '#007bff',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontWeight: '600',
              fontSize: '16px'
            }}>
              {currentUser.username ? currentUser.username.charAt(0).toUpperCase() : 'U'}
            </div>
            <div>
              <div style={{ fontWeight: '600', fontSize: '14px', color: '#343a40' }}>
                {currentUser.username}
              </div>
              <div style={{ fontSize: '12px', color: '#6c757d' }}>
                {currentUser.email || 'No email available'}
              </div>
            </div>
          </div>
        )}
      </div>

          {/* Statistics Cards */}
          <div className="entity-stats-grid" style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
            gap: '20px',
            marginBottom: '24px'
          }}>
        <div style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {registrations.length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Total Registrations</p>
        </div>
        <div style={{
          background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {registrations.filter(r => r.status === 'Submitted').length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Pending Review</p>
        </div>
        <div style={{
          background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {registrations.filter(r => r.status === 'Approved').length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Approved</p>
        </div>
      </div>

          {/* Filters */}
          <div style={{
            backgroundColor: 'white',
            padding: '20px 24px',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            border: '1px solid #e9ecef',
            marginBottom: '20px'
          }}>
            <div className="entity-filters" style={{ 
              display: 'flex', 
              gap: '16px', 
              flexWrap: 'wrap',
              alignItems: 'center',
              justifyContent: 'space-between'
            }}>
        <div className="entity-filter-item" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <label style={{ fontSize: '14px', fontWeight: '500' }}>Status:</label>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            style={{
              padding: '8px 12px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              fontSize: '14px',
              minWidth: '120px'
            }}
          >
            {statusOptions.map(status => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </div>
        
        <input
          type="text"
          placeholder="Search registrations..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="entity-filter-item"
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            fontSize: '14px',
            minWidth: '200px',
            flex: '1',
            maxWidth: '300px'
          }}
        />
            </div>
          </div>

          {/* Registrations Table */}
          <div style={{ 
            backgroundColor: 'white', 
            border: '1px solid #e9ecef', 
            borderRadius: '12px',
            overflow: 'hidden',
            boxShadow: '0 4px 12px rgba(0,0,0,0.1)'
          }}>
        {loading ? (
          <div style={{ 
            padding: '40px', 
            textAlign: 'center', 
            color: '#666',
            fontSize: '16px'
          }}>
            <div style={{ marginBottom: '12px' }}>
              <i style={{ fontSize: '32px', animation: 'spin 1s linear infinite' }}>⏳</i>
            </div>
            Loading registrations for {currentUser?.username || 'current user'}...
          </div>
        ) : filteredRegistrations.length === 0 ? (
          <div style={{ 
            padding: '40px', 
            textAlign: 'center', 
            color: '#666',
            fontSize: '16px'
          }}>
            <div style={{ marginBottom: '12px' }}>
              <i style={{ fontSize: '32px' }}>📋</i>
            </div>
            <h4 style={{ margin: '0 0 8px 0', color: '#495057' }}>No registrations found</h4>
            <p style={{ margin: 0 }}>
              {currentUser ? `No registrations found for ${currentUser.username}.` : 'No registrations available.'}
              {filterStatus !== 'All' && <span> Try changing the status filter.</span>}
              {searchQuery && <span> Try clearing the search query.</span>}
            </p>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', minWidth: '800px' }}>
            <thead>
              <tr style={{ backgroundColor: '#f8f9fa' }}>
                <th 
                  onClick={() => handleSort('institutionName')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  Institution Name {getSortIcon('institutionName')}
                </th>
                <th 
                  onClick={() => handleSort('licenseNumber')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  License Number {getSortIcon('licenseNumber')}
                </th>
                <th 
                  onClick={() => handleSort('status')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'center', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  Status {getSortIcon('status')}
                </th>
                <th 
                  onClick={() => handleSort('createdOnUtc')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  Created Date {getSortIcon('createdOnUtc')}
                </th>
                <th style={{ 
                  padding: '12px', 
                  textAlign: 'center', 
                  borderBottom: '1px solid #e0e0e0',
                  fontSize: '14px',
                  fontWeight: '600'
                }}>
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredRegistrations.map((registration, index) => (
                <tr 
                  key={registration.id}
                  style={{ 
                    backgroundColor: index % 2 === 0 ? '#ffffff' : '#f8f9fa',
                    borderBottom: '1px solid #e0e0e0'
                  }}
                >
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    fontWeight: '500'
                  }}>
                    {registration.institutionName}
                  </td>
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    fontFamily: 'monospace'
                  }}>
                    {registration.licenseNumber}
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <span
                      style={{
                        padding: '4px 8px',
                        borderRadius: '12px',
                        fontSize: '12px',
                        fontWeight: '500',
                        backgroundColor: statusColors[registration.status] + '20',
                        color: statusColors[registration.status]
                      }}
                    >
                      {registration.status}
                    </span>
                  </td>
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    color: '#666'
                  }}>
                    {formatDate(registration.createdOnUtc)}
                  </td>
                  <td style={{ padding: '12px' }}>
                    <div className="entity-table-actions" style={{ 
                      display: 'flex', 
                      gap: '4px', 
                      justifyContent: 'center',
                      flexWrap: 'wrap'
                    }}>
                      {getActionButtons(registration)}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
        )}
          </div>

      {/* Detail Modal */}
      <Modal
        isOpen={detailModalOpen}
        onRequestClose={() => setDetailModalOpen(false)}
        style={modalStyles}
        contentLabel="Registration Details"
      >
        <div className="entity-modal-content" style={{ 
          padding: '24px',
          backgroundColor: 'white'
        }}>
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center', 
            marginBottom: '20px',
            borderBottom: '1px solid #e0e0e0',
            paddingBottom: '16px'
          }}>
            <h3 style={{ margin: 0, fontSize: '20px', fontWeight: '600' }}>
              Registration Details
            </h3>
            <button
              onClick={() => setDetailModalOpen(false)}
              style={{
                background: 'none',
                border: 'none',
                fontSize: '24px',
                cursor: 'pointer',
                color: '#666',
                padding: '0'
              }}
            >
              ×
            </button>
          </div>

          {selectedRegistration && (
            <div style={{ display: 'grid', gap: '20px' }}>
              {/* Basic Information */}
              <div>
                <h4 style={{ margin: '0 0 12px 0', color: '#007bff' }}>Basic Information</h4>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }}>
                  <div>
                    <strong>Institution Name:</strong><br />
                    {selectedRegistration.institutionName}
                  </div>
                  <div>
                    <strong>License Number:</strong><br />
                    {selectedRegistration.licenseNumber}
                  </div>
                  <div>
                    <strong>Country:</strong><br />
                    {selectedRegistration.countryName}
                  </div>
                  <div>
                    <strong>Created By:</strong><br />
                    {selectedRegistration.createdByUserName}
                  </div>
                </div>
              </div>

              {/* Contacts */}
              {selectedRegistration.contacts && selectedRegistration.contacts.length > 0 && (
                <div>
                  <h4 style={{ margin: '0 0 12px 0', color: '#007bff' }}>Contacts</h4>
                  <div style={{ display: 'grid', gap: '8px' }}>
                    {selectedRegistration.contacts.map((contact, index) => (
                      <div key={index} style={{ 
                        padding: '12px', 
                        border: '1px solid #e0e0e0', 
                        borderRadius: '4px',
                        backgroundColor: '#f8f9fa'
                      }}>
                        <strong>{contact.name}</strong> - {contact.position}<br />
                        <small>📧 {contact.email} | 📱 {contact.phone}</small>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Documents */}
              {selectedRegistration.documents && selectedRegistration.documents.length > 0 && (
                <div>
                  <h4 style={{ margin: '0 0 12px 0', color: '#007bff' }}>Documents</h4>
                  <div style={{ display: 'grid', gap: '8px' }}>
                    {selectedRegistration.documents.map((doc, index) => (
                      <div key={index} style={{ 
                        padding: '12px', 
                        border: '1px solid #e0e0e0', 
                        borderRadius: '4px',
                        backgroundColor: '#f8f9fa',
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center'
                      }}>
                        <div>
                          <strong>{doc.name}</strong><br />
                          <small>📄 {doc.fileName} | 📅 {formatDate(doc.uploadedDate)}</small>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Status History */}
              {selectedRegistration.statusLogs && selectedRegistration.statusLogs.length > 0 && (
                <div>
                  <h4 style={{ margin: '0 0 12px 0', color: '#007bff' }}>Status History</h4>
                  <div style={{ display: 'grid', gap: '8px' }}>
                    {selectedRegistration.statusLogs.map((log, index) => (
                      <div key={index} style={{ 
                        padding: '12px', 
                        border: '1px solid #e0e0e0', 
                        borderRadius: '4px',
                        backgroundColor: '#f8f9fa'
                      }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <span style={{ 
                            padding: '2px 6px', 
                            borderRadius: '8px', 
                            fontSize: '11px', 
                            fontWeight: '500',
                            backgroundColor: statusColors[log.status] + '20',
                            color: statusColors[log.status]
                          }}>
                            {log.status}
                          </span>
                          <small>{formatDate(log.date)}</small>
                        </div>
                        <div style={{ marginTop: '8px' }}>
                          <strong>Remarks:</strong> {log.remarks}<br />
                          <strong>Changed by:</strong> {log.changedBy}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </Modal>

      {/* Action Modal */}
      <Modal
        isOpen={actionModalOpen}
        onRequestClose={() => setActionModalOpen(false)}
        style={{
          ...modalStyles,
          content: { ...modalStyles.content, maxWidth: '500px' }
        }}
        contentLabel="Action Confirmation"
      >
        <div style={{ 
          padding: '24px',
          backgroundColor: 'white'
        }}>
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center', 
            marginBottom: '20px',
            borderBottom: '1px solid #e0e0e0',
            paddingBottom: '16px'
          }}>
            <h3 style={{ margin: 0, fontSize: '20px', fontWeight: '600' }}>
              {actionType.charAt(0).toUpperCase() + actionType.slice(1)} Registration
            </h3>
            <button
              onClick={() => setActionModalOpen(false)}
              style={{
                background: 'none',
                border: 'none',
                fontSize: '24px',
                cursor: 'pointer',
                color: '#666',
                padding: '0'
              }}
            >
              ×
            </button>
          </div>

          <div style={{ marginBottom: '20px' }}>
            <p>Are you sure you want to {actionType} this registration?</p>
            {selectedRegistration && (
              <div style={{ 
                padding: '12px', 
                backgroundColor: '#f8f9fa', 
                borderRadius: '4px',
                marginBottom: '16px'
              }}>
                <strong>{selectedRegistration.institutionName}</strong><br />
                License: {selectedRegistration.licenseNumber}
              </div>
            )}
            
            <div>
              <label style={{ 
                display: 'block', 
                marginBottom: '4px', 
                fontSize: '14px', 
                fontWeight: '500' 
              }}>
                Remarks {(actionType === 'reject' || actionType === 'returnForEdit') && '*'}
              </label>
              <textarea
                value={actionRemarks}
                onChange={(e) => setActionRemarks(e.target.value)}
                required={actionType === 'reject' || actionType === 'returnForEdit'}
                rows={3}
                style={{
                  width: '100%',
                  padding: '8px 12px',
                  border: '1px solid #ddd',
                  borderRadius: '4px',
                  fontSize: '14px',
                  resize: 'vertical'
                }}
                placeholder="Enter your remarks..."
              />
            </div>
          </div>

          <div style={{ 
            display: 'flex', 
            gap: '12px', 
            justifyContent: 'flex-end'
          }}>
            <button
              onClick={() => setActionModalOpen(false)}
              style={{
                backgroundColor: '#6c757d',
                color: 'white',
                border: 'none',
                padding: '10px 20px',
                borderRadius: '6px',
                cursor: 'pointer',
                fontSize: '14px'
              }}
            >
              Cancel
            </button>
            <button
              onClick={executeAction}
              disabled={loading || ((actionType === 'reject' || actionType === 'returnForEdit') && !actionRemarks.trim())}
              style={{
                backgroundColor: '#007bff',
                color: 'white',
                border: 'none',
                padding: '10px 20px',
                borderRadius: '6px',
                cursor: loading || ((actionType === 'reject' || actionType === 'returnForEdit') && !actionRemarks.trim()) ? 'not-allowed' : 'pointer',
                fontSize: '14px',
                opacity: loading || ((actionType === 'reject' || actionType === 'returnForEdit') && !actionRemarks.trim()) ? 0.6 : 1
              }}
            >
              {loading ? 'Processing...' : `${actionType.charAt(0).toUpperCase() + actionType.slice(1)}`}
            </button>
          </div>
        </div>
      </Modal>
        </div>
      </div>
    </>
  );
};

export default EntityProfile;