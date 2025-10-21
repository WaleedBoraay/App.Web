import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import Modal from 'react-modal';
import { resourcesApi, languagesApi } from '../../services/apiService';

// Set app element for Modal accessibility
if (typeof window !== 'undefined') {
  Modal.setAppElement(document.getElementById('root') || document.body);
}

const ResourcesManagement = () => {
  const [resources, setResources] = useState([]);
  const [languages, setLanguages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingResource, setEditingResource] = useState(null);
  const [selectedLanguageId, setSelectedLanguageId] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [formData, setFormData] = useState({
    resourceName: '',
    resourceValue: '',
    languageId: 1
  });
  const [sortField, setSortField] = useState('resourceName');
  const [sortDirection, setSortDirection] = useState('asc');

  const loadLanguages = async () => {
    try {
      const data = await languagesApi.getAll();
      setLanguages(Array.isArray(data) ? data.filter(lang => lang.published) : []);
    } catch (error) {
      console.error('Error loading languages:', error);
      toast.error('Failed to load languages');
    }
  };

  const loadResources = useCallback(async () => {
    setLoading(true);
    try {
      const data = await resourcesApi.getAll(selectedLanguageId, searchQuery || null);
      setResources(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error('Error loading resources:', error);
      toast.error('Failed to load resources');
    } finally {
      setLoading(false);
    }
  }, [selectedLanguageId, searchQuery]);

  useEffect(() => {
    loadLanguages();
  }, []);

  useEffect(() => {
    loadResources();
  }, [loadResources]);

  const handleCreate = () => {
    setEditingResource(null);
    setFormData({
      resourceName: '',
      resourceValue: '',
      languageId: selectedLanguageId
    });
    setModalOpen(true);
  };

  const handleEdit = (resource) => {
    setEditingResource(resource);
    setFormData({
      resourceName: resource.resourceName || '',
      resourceValue: resource.resourceValue || '',
      languageId: resource.languageId || selectedLanguageId
    });
    setModalOpen(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingResource) {
        await resourcesApi.update(editingResource.id, formData);
        toast.success('Resource updated successfully');
      } else {
        await resourcesApi.create(formData);
        toast.success('Resource created successfully');
      }
      
      setModalOpen(false);
      loadResources();
    } catch (error) {
      console.error('Error saving resource:', error);
      if (error.response?.status === 409) {
        toast.error('Resource already exists');
      } else {
        toast.error(`Failed to ${editingResource ? 'update' : 'create'} resource`);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (resource) => {
    if (!window.confirm(`Are you sure you want to delete "${resource.resourceName}"?`)) {
      return;
    }

    setLoading(true);
    try {
      await resourcesApi.delete(resource.id, resource.languageId);
      toast.success('Resource deleted successfully');
      loadResources();
    } catch (error) {
      console.error('Error deleting resource:', error);
      toast.error('Failed to delete resource');
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

  const getSelectedLanguageName = () => {
    const lang = languages.find(l => l.id === selectedLanguageId);
    return lang ? lang.name : 'Select Language';
  };

  const sortedResources = resources
    .sort((a, b) => {
      const aVal = a[sortField] || '';
      const bVal = b[sortField] || '';
      
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
      backgroundColor: 'rgba(0, 0, 0, 0.5)',
      zIndex: 1000
    },
    content: {
      top: '50%',
      left: '50%',
      right: 'auto',
      bottom: 'auto',
      marginRight: '-50%',
      transform: 'translate(-50%, -50%)',
      padding: '0',
      border: 'none',
      borderRadius: '8px',
      maxWidth: '600px',
      width: '90%',
      maxHeight: '90vh',
      overflow: 'auto'
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center', 
        marginBottom: '24px',
        flexWrap: 'wrap',
        gap: '12px'
      }}>
        <div>
          <h2 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '600' }}>
            Resources Management
          </h2>
          <p style={{ margin: '0', color: '#666', fontSize: '14px' }}>
            Manage localization resources and translations
          </p>
        </div>
        <button
          onClick={handleCreate}
          disabled={loading}
          style={{
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            padding: '10px 20px',
            borderRadius: '6px',
            cursor: loading ? 'not-allowed' : 'pointer',
            fontSize: '14px',
            fontWeight: '500',
            opacity: loading ? 0.6 : 1,
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          <span>➕</span>
          Add Resource
        </button>
      </div>

      {/* Statistics Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
        gap: '16px',
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
            {resources.length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Total Resources</p>
        </div>
        <div style={{
          background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {languages.length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Available Languages</p>
        </div>
        <div style={{
          background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {getSelectedLanguageName()}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Current Language</p>
        </div>
      </div>

      {/* Filters */}
      <div style={{ 
        display: 'flex', 
        gap: '16px', 
        marginBottom: '20px',
        flexWrap: 'wrap',
        alignItems: 'center'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <label style={{ fontSize: '14px', fontWeight: '500' }}>Language:</label>
          <select
            value={selectedLanguageId}
            onChange={(e) => setSelectedLanguageId(parseInt(e.target.value))}
            style={{
              padding: '8px 12px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              fontSize: '14px',
              minWidth: '150px'
            }}
          >
            {languages.map(lang => (
              <option key={lang.id} value={lang.id}>
                {lang.name} ({lang.uniqueSeoCode})
              </option>
            ))}
          </select>
        </div>
        
        <input
          type="text"
          placeholder="Search resources..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            fontSize: '14px',
            minWidth: '200px'
          }}
        />
      </div>

      {/* Resources Table */}
      <div style={{ 
        backgroundColor: 'white', 
        border: '1px solid #e0e0e0', 
        borderRadius: '8px',
        overflow: 'hidden',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
      }}>
        {loading ? (
          <div style={{ 
            padding: '40px', 
            textAlign: 'center', 
            color: '#666',
            fontSize: '16px'
          }}>
            Loading resources...
          </div>
        ) : sortedResources.length === 0 ? (
          <div style={{ 
            padding: '40px', 
            textAlign: 'center', 
            color: '#666',
            fontSize: '16px'
          }}>
            No resources found
          </div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ backgroundColor: '#f8f9fa' }}>
                <th 
                  onClick={() => handleSort('resourceName')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600',
                    width: '30%'
                  }}
                >
                  Resource Name {getSortIcon('resourceName')}
                </th>
                <th 
                  onClick={() => handleSort('resourceValue')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600',
                    width: '50%'
                  }}
                >
                  Resource Value {getSortIcon('resourceValue')}
                </th>
                <th style={{ 
                  padding: '12px', 
                  textAlign: 'center', 
                  borderBottom: '1px solid #e0e0e0',
                  fontSize: '14px',
                  fontWeight: '600',
                  width: '20%'
                }}>
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {sortedResources.map((resource, index) => (
                <tr 
                  key={resource.id}
                  style={{ 
                    backgroundColor: index % 2 === 0 ? '#ffffff' : '#f8f9fa',
                    borderBottom: '1px solid #e0e0e0'
                  }}
                >
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    fontFamily: 'monospace',
                    fontWeight: '500'
                  }}>
                    {resource.resourceName}
                  </td>
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    maxWidth: '300px',
                    wordBreak: 'break-word'
                  }}>
                    {resource.resourceValue || <span style={{ color: '#999', fontStyle: 'italic' }}>No value</span>}
                  </td>
                  <td style={{ padding: '12px' }}>
                    <div style={{ 
                      display: 'flex', 
                      gap: '8px', 
                      justifyContent: 'center',
                      flexWrap: 'wrap'
                    }}>
                      <button
                        onClick={() => handleEdit(resource)}
                        style={{
                          backgroundColor: '#ffc107',
                          color: '#000',
                          border: 'none',
                          padding: '6px 12px',
                          borderRadius: '4px',
                          cursor: 'pointer',
                          fontSize: '12px',
                          fontWeight: '500'
                        }}
                      >
                        ✏️ Edit
                      </button>
                      <button
                        onClick={() => handleDelete(resource)}
                        style={{
                          backgroundColor: '#dc3545',
                          color: 'white',
                          border: 'none',
                          padding: '6px 12px',
                          borderRadius: '4px',
                          cursor: 'pointer',
                          fontSize: '12px',
                          fontWeight: '500'
                        }}
                      >
                        🗑️ Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Resource Form Modal */}
      <Modal
        isOpen={modalOpen}
        onRequestClose={() => setModalOpen(false)}
        style={modalStyles}
        contentLabel={editingResource ? "Edit Resource" : "Create Resource"}
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
              {editingResource ? 'Edit Resource' : 'Create New Resource'}
            </h3>
            <button
              onClick={() => setModalOpen(false)}
              style={{
                background: 'none',
                border: 'none',
                fontSize: '24px',
                cursor: 'pointer',
                color: '#666',
                padding: '0',
                width: '30px',
                height: '30px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}
            >
              ×
            </button>
          </div>

          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gap: '16px' }}>
              <div>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '4px', 
                  fontSize: '14px', 
                  fontWeight: '500' 
                }}>
                  Language *
                </label>
                <select
                  value={formData.languageId}
                  onChange={(e) => setFormData({ ...formData, languageId: parseInt(e.target.value) })}
                  required
                  style={{
                    width: '100%',
                    padding: '8px 12px',
                    border: '1px solid #ddd',
                    borderRadius: '4px',
                    fontSize: '14px'
                  }}
                >
                  {languages.map(lang => (
                    <option key={lang.id} value={lang.id}>
                      {lang.name} ({lang.uniqueSeoCode})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '4px', 
                  fontSize: '14px', 
                  fontWeight: '500' 
                }}>
                  Resource Name *
                </label>
                <input
                  type="text"
                  value={formData.resourceName}
                  onChange={(e) => setFormData({ ...formData, resourceName: e.target.value })}
                  required
                  style={{
                    width: '100%',
                    padding: '8px 12px',
                    border: '1px solid #ddd',
                    borderRadius: '4px',
                    fontSize: '14px',
                    fontFamily: 'monospace'
                  }}
                  placeholder="e.g., Common.Welcome, Admin.Users.Title"
                />
                <small style={{ color: '#666', fontSize: '12px' }}>
                  Use dot notation for hierarchical organization (e.g., Common.Welcome, Admin.Users.Title)
                </small>
              </div>

              <div>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '4px', 
                  fontSize: '14px', 
                  fontWeight: '500' 
                }}>
                  Resource Value
                </label>
                <textarea
                  value={formData.resourceValue}
                  onChange={(e) => setFormData({ ...formData, resourceValue: e.target.value })}
                  rows={4}
                  style={{
                    width: '100%',
                    padding: '8px 12px',
                    border: '1px solid #ddd',
                    borderRadius: '4px',
                    fontSize: '14px',
                    resize: 'vertical'
                  }}
                  placeholder="Enter the translated text for this resource"
                />
              </div>
            </div>

            <div style={{ 
              display: 'flex', 
              gap: '12px', 
              justifyContent: 'flex-end',
              marginTop: '24px',
              paddingTop: '16px',
              borderTop: '1px solid #e0e0e0'
            }}>
              <button
                type="button"
                onClick={() => setModalOpen(false)}
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
                type="submit"
                disabled={loading}
                style={{
                  backgroundColor: '#007bff',
                  color: 'white',
                  border: 'none',
                  padding: '10px 20px',
                  borderRadius: '6px',
                  cursor: loading ? 'not-allowed' : 'pointer',
                  fontSize: '14px',
                  opacity: loading ? 0.6 : 1
                }}
              >
                {loading ? 'Saving...' : (editingResource ? 'Update Resource' : 'Create Resource')}
              </button>
            </div>
          </form>
        </div>
      </Modal>
    </div>
  );
};

export default ResourcesManagement;