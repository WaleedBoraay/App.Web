import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { permissionsApi } from '../../services/apiService';

const PermissionManagement = ({ isMobile }) => {
  const [permissions, setPermissions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const [permissionCategories, setPermissionCategories] = useState({});

  // Load permissions
  const loadPermissions = async () => {
    setLoading(true);
    try {
      const response = await permissionsApi.getAll();
      setPermissions(response || []);
      
      // Group permissions by category
      const categories = {};
      response?.forEach(permission => {
        if (!categories[permission.category]) {
          categories[permission.category] = [];
        }
        categories[permission.category].push(permission);
      });
      setPermissionCategories(categories);
      
      toast.success('Permissions loaded successfully');
    } catch (error) {
      console.error('Error loading permissions:', error);
      toast.error('Failed to load permissions');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPermissions();
  }, []);

  // Handle permission sync
  const handleSyncPermissions = async () => {
    setSyncing(true);
    try {
      const response = await permissionsApi.syncPermissions();
      if (response.success) {
        toast.success(response.message || 'Permissions synchronized successfully');
        await loadPermissions(); // Reload permissions after sync
      }
    } catch (error) {
      console.error('Error syncing permissions:', error);
      toast.error('Failed to synchronize permissions');
    } finally {
      setSyncing(false);
    }
  };

  // Filter permissions
  const filteredPermissions = permissions.filter(permission => {
    const matchesSearch = permission.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         permission.systemName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         permission.description.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesCategory = categoryFilter === 'all' || permission.category === categoryFilter;
    const matchesStatus = statusFilter === 'all' || 
                         (statusFilter === 'active' && permission.isActive) ||
                         (statusFilter === 'inactive' && !permission.isActive);

    return matchesSearch && matchesCategory && matchesStatus;
  });

  // Get unique categories for filter dropdown
  const uniqueCategories = [...new Set(permissions.map(p => p.category))].sort();

  // Get statistics
  const stats = {
    total: permissions.length,
    active: permissions.filter(p => p.isActive).length,
    inactive: permissions.filter(p => !p.isActive).length,
    categories: uniqueCategories.length
  };

  if (loading && permissions.length === 0) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        minHeight: '400px',
        flexDirection: 'column'
      }}>
        <div style={{
          border: '3px solid #f3f3f3',
          borderTop: '3px solid #20ABA0',
          borderRadius: '50%',
          width: '40px',
          height: '40px',
          animation: 'spin 1s linear infinite',
          marginBottom: '1rem'
        }}></div>
        <p style={{ color: '#6c757d' }}>Loading permissions...</p>
      </div>
    );
  }

  return (
    <div style={{ padding: isMobile ? '1rem' : '1.5rem' }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '2rem',
        flexDirection: isMobile ? 'column' : 'row',
        gap: isMobile ? '1rem' : '0'
      }}>
        <div>
          <h2 style={{
            color: '#2c3e50',
            fontSize: isMobile ? '1.5rem' : '1.8rem',
            fontWeight: '600',
            margin: '0'
          }}>
            <i className="fas fa-key" style={{ marginRight: '0.5rem', color: '#20ABA0' }}></i>
            Permission Management
          </h2>
          <p style={{
            color: '#6c757d',
            fontSize: '0.95rem',
            margin: '0.5rem 0 0 0'
          }}>
            Manage system permissions and synchronize with codebase
          </p>
        </div>

        <button
          onClick={handleSyncPermissions}
          disabled={syncing}
          style={{
            background: 'linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%)',
            color: 'white',
            border: 'none',
            padding: '0.75rem 1.5rem',
            borderRadius: '8px',
            fontSize: '0.95rem',
            fontWeight: '600',
            cursor: syncing ? 'not-allowed' : 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
            opacity: syncing ? 0.7 : 1,
            transition: 'all 0.3s ease',
            width: isMobile ? '100%' : 'auto',
            justifyContent: 'center'
          }}
        >
          <i className={`fas fa-${syncing ? 'spinner fa-spin' : 'sync-alt'}`}></i>
          {syncing ? 'Syncing...' : 'Sync Permissions'}
        </button>
      </div>

      {/* Statistics Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: isMobile ? 'repeat(2, 1fr)' : 'repeat(4, 1fr)',
        gap: '1rem',
        marginBottom: '2rem'
      }}>
        <div style={{
          backgroundColor: 'white',
          padding: '1.5rem',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          textAlign: 'center'
        }}>
          <div style={{
            fontSize: '2rem',
            fontWeight: '700',
            color: '#20ABA0',
            marginBottom: '0.5rem'
          }}>
            {stats.total}
          </div>
          <div style={{
            color: '#6c757d',
            fontSize: '0.9rem',
            fontWeight: '600'
          }}>
            Total Permissions
          </div>
        </div>

        <div style={{
          backgroundColor: 'white',
          padding: '1.5rem',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          textAlign: 'center'
        }}>
          <div style={{
            fontSize: '2rem',
            fontWeight: '700',
            color: '#28a745',
            marginBottom: '0.5rem'
          }}>
            {stats.active}
          </div>
          <div style={{
            color: '#6c757d',
            fontSize: '0.9rem',
            fontWeight: '600'
          }}>
            Active
          </div>
        </div>

        <div style={{
          backgroundColor: 'white',
          padding: '1.5rem',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          textAlign: 'center'
        }}>
          <div style={{
            fontSize: '2rem',
            fontWeight: '700',
            color: '#dc3545',
            marginBottom: '0.5rem'
          }}>
            {stats.inactive}
          </div>
          <div style={{
            color: '#6c757d',
            fontSize: '0.9rem',
            fontWeight: '600'
          }}>
            Inactive
          </div>
        </div>

        <div style={{
          backgroundColor: 'white',
          padding: '1.5rem',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          textAlign: 'center'
        }}>
          <div style={{
            fontSize: '2rem',
            fontWeight: '700',
            color: '#6f42c1',
            marginBottom: '0.5rem'
          }}>
            {stats.categories}
          </div>
          <div style={{
            color: '#6c757d',
            fontSize: '0.9rem',
            fontWeight: '600'
          }}>
            Categories
          </div>
        </div>
      </div>

      {/* Filters */}
      <div style={{
        display: 'flex',
        gap: '1rem',
        marginBottom: '2rem',
        flexDirection: isMobile ? 'column' : 'row',
        alignItems: isMobile ? 'stretch' : 'center'
      }}>
        <div style={{ flex: 1, minWidth: '200px' }}>
          <input
            type="text"
            placeholder="Search permissions..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              width: '100%',
              padding: '0.75rem 1rem',
              border: '1px solid #ddd',
              borderRadius: '8px',
              fontSize: '0.95rem'
            }}
          />
        </div>
        
        <select
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
          style={{
            padding: '0.75rem 1rem',
            border: '1px solid #ddd',
            borderRadius: '8px',
            fontSize: '0.95rem',
            minWidth: '150px'
          }}
        >
          <option value="all">All Categories</option>
          {uniqueCategories.map(category => (
            <option key={category} value={category}>{category}</option>
          ))}
        </select>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          style={{
            padding: '0.75rem 1rem',
            border: '1px solid #ddd',
            borderRadius: '8px',
            fontSize: '0.95rem',
            minWidth: '120px'
          }}
        >
          <option value="all">All Status</option>
          <option value="active">Active Only</option>
          <option value="inactive">Inactive Only</option>
        </select>
      </div>

      {/* Permissions Display */}
      {filteredPermissions.length === 0 ? (
        <div style={{
          textAlign: 'center',
          padding: '3rem',
          color: '#6c757d',
          backgroundColor: '#f8f9fa',
          borderRadius: '12px',
          border: '2px dashed #dee2e6'
        }}>
          <i className="fas fa-key" style={{ fontSize: '3rem', marginBottom: '1rem', color: '#dee2e6' }}></i>
          <h3 style={{ marginBottom: '0.5rem' }}>No Permissions Found</h3>
          <p>
            {searchTerm || categoryFilter !== 'all' || statusFilter !== 'all'
              ? 'No permissions match your current filters.'
              : 'No permissions have been loaded yet. Try syncing permissions.'
            }
          </p>
        </div>
      ) : (
        <div style={{
          display: 'grid',
          gridTemplateColumns: isMobile ? '1fr' : 'repeat(auto-fill, minmax(350px, 1fr))',
          gap: '1.5rem'
        }}>
          {filteredPermissions.map((permission) => (
            <div
              key={permission.id}
              style={{
                backgroundColor: 'white',
                borderRadius: '12px',
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: '1px solid #f0f0f0',
                padding: '1.5rem',
                transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                cursor: 'default'
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = 'translateY(-2px)';
                e.target.style.boxShadow = '0 8px 30px rgba(0,0,0,0.12)';
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = 'translateY(0)';
                e.target.style.boxShadow = '0 4px 20px rgba(0,0,0,0.08)';
              }}
            >
              {/* Header */}
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                marginBottom: '1rem'
              }}>
                <div style={{ flex: 1 }}>
                  <h4 style={{
                    margin: '0',
                    color: '#2c3e50',
                    fontSize: '1.1rem',
                    fontWeight: '600',
                    marginBottom: '0.5rem'
                  }}>
                    {permission.name}
                  </h4>
                  <div style={{
                    display: 'flex',
                    gap: '0.5rem',
                    alignItems: 'center',
                    flexWrap: 'wrap'
                  }}>
                    <span style={{
                      padding: '0.25rem 0.75rem',
                      borderRadius: '20px',
                      fontSize: '0.75rem',
                      fontWeight: '600',
                      textTransform: 'uppercase',
                      backgroundColor: permission.isActive ? '#d4edda' : '#f8d7da',
                      color: permission.isActive ? '#155724' : '#721c24',
                      border: `1px solid ${permission.isActive ? '#c3e6cb' : '#f5c6cb'}`
                    }}>
                      {permission.isActive ? 'Active' : 'Inactive'}
                    </span>
                    <span style={{
                      padding: '0.25rem 0.75rem',
                      borderRadius: '20px',
                      fontSize: '0.75rem',
                      fontWeight: '600',
                      backgroundColor: '#e3f2fd',
                      color: '#0d47a1',
                      border: '1px solid #bbdefb'
                    }}>
                      {permission.category}
                    </span>
                  </div>
                </div>
              </div>

              {/* Description */}
              <p style={{
                color: '#495057',
                fontSize: '0.9rem',
                lineHeight: '1.5',
                marginBottom: '1rem'
              }}>
                {permission.description}
              </p>

              {/* System Name */}
              <div style={{
                backgroundColor: '#f8f9fa',
                padding: '0.75rem',
                borderRadius: '6px',
                border: '1px solid #e9ecef'
              }}>
                <div style={{
                  fontSize: '0.8rem',
                  color: '#6c757d',
                  marginBottom: '0.25rem',
                  fontWeight: '600'
                }}>
                  System Name:
                </div>
                <code style={{
                  fontSize: '0.85rem',
                  color: '#e83e8c',
                  fontWeight: '600'
                }}>
                  {permission.systemName}
                </code>
              </div>
            </div>
          ))}
        </div>
      )}

      <style jsx>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
};

export default PermissionManagement;