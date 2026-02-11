package com.tonvq.accountingerp.infrastructure.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;
import org.thymeleaf.spring6.SpringTemplateEngine;
import org.thymeleaf.spring6.view.ThymeleafViewResolver;

/**
 * Thymeleaf Template Configuration
 * Template resolver, view resolver, template engine settings
 */
@Configuration
public class ThymeleafConfig implements WebMvcConfigurer {
    
    /**
     * Thymeleaf Template Engine Bean
     * Configured in application.yml with spring.thymeleaf properties
     * No need for explicit bean configuration - Spring Boot auto-configures it
     */
    
    @Bean
    public ThymeleafViewResolver thymeleafViewResolver(SpringTemplateEngine templateEngine) {
        ThymeleafViewResolver resolver = new ThymeleafViewResolver();
        resolver.setTemplateEngine(templateEngine);
        resolver.setCharacterEncoding("UTF-8");
        resolver.setContentType("text/html;charset=UTF-8");
        resolver.setOrder(1);
        return resolver;
    }
}
