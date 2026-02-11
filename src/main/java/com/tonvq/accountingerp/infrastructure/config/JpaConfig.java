package com.tonvq.accountingerp.infrastructure.config;

import org.springframework.context.annotation.Configuration;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;
import org.springframework.data.jpa.repository.config.EnableJpaAuditing;
import org.springframework.transaction.annotation.EnableTransactionManagement;

/**
 * JPA Configuration
 * Enable JPA repositories, auditing, transaction management
 */
@Configuration
@EnableJpaRepositories(basePackages = "com.tonvq.accountingerp.infrastructure.persistence.repository")
@EnableJpaAuditing
@EnableTransactionManagement
public class JpaConfig {
    // Configuration is done through annotations above
    // Spring Boot auto-configuration handles EntityManager, DataSource, etc.
}
