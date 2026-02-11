package com.tonvq.accountingerp.infrastructure.persistence.repository;

import com.tonvq.accountingerp.infrastructure.persistence.entity.UserEntity;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.Optional;

/**
 * Spring Data JPA Repository - User
 */
public interface JpaUserRepository extends BaseRepository<UserEntity, Long> {
    Optional<UserEntity> findByUsername(String username);
    
    Optional<UserEntity> findByEmail(String email);
    
    @Query("SELECT u FROM UserEntity u WHERE u.username = :username AND u.isActive = true")
    Optional<UserEntity> findActiveByUsername(@Param("username") String username);
}
