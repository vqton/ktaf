package com.tonvq.accountingerp.infrastructure.persistence.repository;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.repository.NoRepositoryBean;
import java.util.List;

/**
 * Base repository interface cho tất cả entities
 * Cung cấp pagination, sorting, custom queries
 */
@NoRepositoryBean
public interface BaseRepository<T, ID> extends JpaRepository<T, ID> {
    List<T> findAllByIsDeletedFalse();
}
